using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using ChestCounter;
using Contador;
using Contador.Engine;

namespace Contador.DesktopChecks;

internal static class TeamViewChecks
{
    public static async Task Run(Application app, string? output = null)
    {
        ViewsPreview.LoadStyles(app);
        app.Resources["TeamVisible"] = new BooleanToVisibilityConverter();
        L.Set("es");
        var layout = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "TeamLayout.xaml"));
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var element = layout.Descendants().Single(e => (string?)e.Attribute(x + "Name") == "TeamSummaryPanel");
        var panel = (StackPanel)XamlReader.Parse(element.ToString());
        var vm = new Dashboard();
        var config = new Config { Profile = "sample", ShowTeamStats = true };
        var data = new TrackerData { Matches = [new MatchRecord
        {
            EpicId = "sample", SessionId = "fixture", ChestsOpened = 39, TeammateChestsOpened = 21,
            TreasureFinds = [new() { SourceKind = ChestKinds.Normal, SourceCertain = true }],
            TeamTreasureFinds = [new() { SourceCertain = true, Teammate = "Compañero de prueba" }]
        }] };
        vm.Refresh(data, config, new SessionCounter(DateTime.UtcNow));
        var content = new StackPanel { Margin = new Thickness(24), DataContext = vm };
        content.Children.Add(new TextBlock
        {
            Text = "VISTA DE PRUEBA · Datos ficticios del equipo", Margin = new Thickness(0, 0, 0, 20)
        });
        content.Children.Add(panel);
        var window = new Window
        {
            Content = new ScrollViewer { Content = content }, Width = 850, Height = 600,
            Left = -20000, Top = -20000, ShowInTaskbar = false, ShowActivated = false
        };
        window.Show();
        try
        {
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            if (panel.Visibility != Visibility.Visible || vm.TeamChestTotal != "≥ 60" || vm.Total != "39")
                throw new Exception("El desglose de equipo no conserva el total personal.");
            config.ShowTeamStats = false;
            vm.Refresh(data, config, new SessionCounter(DateTime.UtcNow));
            await Dispatcher.Yield(DispatcherPriority.DataBind);
            if (panel.Visibility != Visibility.Collapsed) throw new Exception("No se oculta el equipo al desactivarlo.");
            config.ShowTeamStats = true;
            vm.Refresh(new TrackerData(), config, new SessionCounter(DateTime.UtcNow));
            await Dispatcher.Yield(DispatcherPriority.DataBind);
            if (vm.TeamChestTotal != "—" || vm.TeamInfo.CoveredMatches != 0)
                throw new Exception("Estado vacío inventa un recuento del equipo.");
            vm.Refresh(data, config, new SessionCounter(DateTime.UtcNow));
            await Dispatcher.Yield(DispatcherPriority.DataBind);
            if (output != null)
            {
                Directory.CreateDirectory(output);
                foreach (var width in new[] { 850, 600 })
                {
                    window.Width = width;
                    window.UpdateLayout();
                    await Dispatcher.Yield(DispatcherPriority.Render);
                    var bitmap = new RenderTargetBitmap((int)Math.Ceiling(content.ActualWidth),
                        (int)Math.Ceiling(content.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                    var drawing = new DrawingVisual();
                    using (var dc = drawing.RenderOpen())
                    {
                        dc.DrawRectangle((Brush)app.Resources["Ground"], null, new Rect(0, 0, bitmap.Width, bitmap.Height));
                        dc.DrawRectangle(new VisualBrush(content), null, new Rect(0, 0, bitmap.Width, bitmap.Height));
                    }
                    bitmap.Render(drawing);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using var file = File.Create(Path.Combine(output, $"equipo-{width}.png"));
                    encoder.Save(file);
                }
            }
            Console.WriteLine("3 comprobaciones WPF de equipo correctas: desglose, desactivación y estado vacío.");
        }
        finally { window.Close(); }
    }
}
