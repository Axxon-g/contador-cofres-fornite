using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Xml.Linq;
using ChestCounter;
using Contador;
using Contador.Engine;

namespace Contador.DesktopChecks;

internal static class ViewsPreview
{
    public static void LoadStyles(Application app)
    {
        var document = XDocument.Load(System.IO.Path.Combine(AppContext.BaseDirectory, "AppStyles.xaml"));
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        var resources = document.Root!.Element(ns + "Application.Resources")!;
        resources.Name = ns + "ResourceDictionary";
        app.Resources = (ResourceDictionary)XamlReader.Parse(resources.ToString());
    }

    public static void Show(Application app, string? captureFolder = null)
    {
        LoadStyles(app);
        L.Set("es");
        var data = new TrackerData();
        for (var i = 0; i < 12; i++)
        {
            data.Matches.Add(new MatchRecord
            {
                EpicId = "sample", SessionId = "fixture-" + i, PlayerName = "Cuenta de prueba",
                MatchStartUtc = DateTime.UtcNow.AddDays(-i), AnalyzedAtUtc = DateTime.UtcNow,
                ChestsOpened = 12 + i, Playlist = "Playlist_NoBuildBR_Squad", Placement = i + 1,
                Chests = Enumerable.Range(0, 12 + i).Select(c => new ChestRecord
                {
                    Kind = c % 3 == 0 ? "Coffre rare" : "Coffre", Destroyed = c == 0
                }).ToList(),
                AmmoBoxesOpened = 3,
                TreasureFinds = i % 4 == 0 ? [new TreasureFind
                {
                    Item = "Tesoro de prueba", ObtainedAtSeconds = 135,
                    SourceKind = "Coffre", SourceCertain = true, TimeExact = true, FromGoldenChest = true
                }] : [],
                TeamTreasureFinds = i == 0 ? [new TreasureFind
                {
                    Item = "Hallazgo de equipo de prueba", Teammate = "Compañero de prueba", SourceKind = "Coffre"
                }] : []
            });
        }
        var vm = new Dashboard();
        var config = new Config { Profile = "sample" };
        var session = new SessionCounter(DateTime.UtcNow.AddHours(-1));
        vm.Refresh(data, config, session);
        var root = new DockPanel { Margin = new Thickness(28), Background = (Brush)app.Resources["Ground"] };
        var bar = new WrapPanel { Margin = new Thickness(0, 0, 0, 16) };
        bar.Children.Add(new TextBlock { Text = "VISTA DE PRUEBA · Informe con datos ficticios", Margin = new Thickness(0, 10, 16, 0) });
        DockPanel.SetDock(bar, Dock.Top);
        root.Children.Add(bar);
        var host = new ContentControl();
        root.Children.Add(host);
        var routes = new RouteSuggestionsView(false);
        var report = new PlayerReportView { DataContext = vm };
        host.Content = routes;
        var window = new Window
        {
            Title = "Contador 1.2.7 · Rutas e informe · Vista de prueba",
            Width = 1000, Height = 900, MinWidth = 620, Content = root,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        void Button(string text, Action action)
        {
            var button = new Button { Content = text, Padding = new Thickness(12, 8, 12, 8) };
            button.Click += (_, _) => action();
            bar.Children.Add(button);
        }
        Button("Rutas", () => host.Content = routes);
        Button("Informe", () => host.Content = report);
        Button("Ancho reducido", () => window.Width = 640);
        Button("Ancho normal", () => window.Width = 1000);
        Button("Sin datos", () => vm.Refresh(new TrackerData(), config, session));
        Button("Con datos", () => vm.Refresh(data, config, session));
        window.Closed += (_, _) => app.Shutdown();
        if (captureFolder != null)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = window.Top = -20000;
            window.ShowActivated = false;
            window.ShowInTaskbar = false;
            window.Loaded += async (_, _) =>
            {
                Directory.CreateDirectory(captureFolder);
                async Task Save(string name, double width, UserControl view, double offset = 0)
                {
                    window.Width = width;
                    host.Content = view;
                    await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                    if (view.Content is ScrollViewer scroll) scroll.ScrollToVerticalOffset(offset);
                    window.UpdateLayout();
                    await Dispatcher.Yield(DispatcherPriority.Render);
                    var bitmap = new RenderTargetBitmap((int)Math.Ceiling(root.ActualWidth),
                        (int)Math.Ceiling(root.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                    var drawing = new DrawingVisual();
                    using (var context = drawing.RenderOpen())
                    {
                        context.DrawRectangle(new VisualBrush(root), null,
                            new Rect(0, 0, root.ActualWidth, root.ActualHeight));
                    }
                    bitmap.Render(drawing);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using var output = File.Create(Path.Combine(captureFolder, name + ".png"));
                    encoder.Save(output);
                }
                await Save("rutas-desktop", 1000, routes);
                await Save("rutas-minimo", 640, routes);
                routes.Planner.SelectedGuide = RouteGuides.All[0];
                await Save("mapa-ruta", 1000, routes);
                var point = MapCatalog.Current.Points.First(p => p.Kind == "buried_chest");
                routes.Planner.SelectGroup([point]);
                routes.MapControl.FocusPoint(point.X, point.Y, 8);
                await Save("mapa-zoom", 1000, routes);
                routes.Planner.Mark(MapReview.Checked);
                routes.Planner.HideReviewed = true;
                routes.Planner.Search = "sin resultados de prueba";
                await Save("mapa-vacio", 640, routes);
                routes.Planner.Search = "";
                routes.Planner.HideReviewed = false;
                routes.Planner.SelectedGuide = RouteGuides.All[3];
                routes.MapControl.Fit();
                await Save("rutas-alternativa", 1000, routes);
                await Save("informe-desktop", 1000, report);
                await Save("informe-minimo", 640, report);
                vm.ShowTeamTreasures = true;
                await Save("informe-equipo", 1000, report, 1100);
                await Save("informe-detalle-minimo", 640, report, 1000);
                vm.Refresh(new TrackerData(), config, session);
                await Save("informe-vacio", 640, report);
                Console.WriteLine("11 vistas WPF renderizadas sin acceso al historial del usuario.");
                window.Close();
            };
        }
        window.Show();
    }
}
