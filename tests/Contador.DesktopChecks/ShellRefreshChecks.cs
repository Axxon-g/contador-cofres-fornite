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

internal static class ShellRefreshChecks
{
    public static async Task Run(Application app, string? output)
    {
        ViewsPreview.LoadStyles(app);
        L.Set("es");
        var doc = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "TeamLayout.xaml"));
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        XNamespace ui = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        doc.Root!.Attribute(x + "Class")!.Remove();
        foreach (var item in doc.Descendants())
        {
            foreach (var attribute in item.Attributes().Where(attribute =>
                attribute.Name.LocalName is "Click" or "SelectionChanged" or "DateValidationError" or "ToggleRequested" or "HistoryRequested" or "ReportRequested" or "StatisticsRequested" or "ToggleLiveRequested").ToList())
                attribute.Remove();
            if (item.Name.LocalName is "RouteSuggestionsView" or "PlayerReportView" or "PlayerStatisticsView" or "TreasureOddsView" or "SummaryView")
                item.Name = ui + "ContentControl"; // Prevent any constructor that reads local plans.
        }
        var text = doc.ToString().Replace("clr-namespace:Contador", "clr-namespace:Contador;assembly=Contador");
        var window = (Window)XamlReader.Parse(text);
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = window.Top = -20000;
        window.ShowActivated = false;
        window.ShowInTaskbar = false;
        var vm = new Dashboard();
        var matches = new TrackerData();
        var primary = new MatchRecord
        {
            SessionId = "fixture-session",
            SessionIdentityVerified = true,
            EpicId = "sample",
            PlayerName = "Cuenta de prueba",
            ChestsOpened = 33,
            TeammateChestsOpened = 46,
            IsReplayOwner = true,
            AnalyzerVersion = ReplayAnalyzer.CurrentVersion,
            MatchStartUtc = new DateTime(2026, 11, 15, 20, 0, 0, DateTimeKind.Utc),
            Playlist = "Playlist_NoBuildBR_Squad",
            TeamChests = [new() { EpicId = "mate-a", PlayerName = "Marina de prueba", Chests = 9 },
                new() { EpicId = "mate-b", PlayerName = "Leo de prueba", Chests = 12 },
                new() { EpicId = "mate-c", PlayerName = "Alex de prueba", Chests = 25 }]
        };
        matches.Matches.Add(primary);
        for (var i = 1; i <= 6; i++)
            matches.Matches.Add(new MatchRecord
            {
                SessionId = "fixture-" + i,
                EpicId = "sample",
                PlayerName = "Cuenta de prueba",
                ChestsOpened = 12 + i,
                MatchStartUtc = primary.MatchStartUtc!.Value.AddDays(-i),
                Playlist = "Playlist_NoBuildBR_Squad"
            });
        matches.TeamReplaySources = [new MatchRecord
        {
            SessionId = primary.SessionId, SessionIdentityVerified = true, EpicId = "mate-a", PlayerName = "Marina de prueba",
            IsReplayOwner = true, AnalyzerVersion = ReplayAnalyzer.CurrentVersion, ChestsOpened = 22,
            ReplayFile = "repeticion-de-prueba-a.replay",
            TeamChests = [new() { EpicId = "mate-b", PlayerName = "Leo de prueba", Chests = 17 }]
        }, new MatchRecord
        {
            SessionId = primary.SessionId, SessionIdentityVerified = true, EpicId = "mate-c", PlayerName = "Alex de prueba",
            IsReplayOwner = true, AnalyzerVersion = ReplayAnalyzer.CurrentVersion, ChestsOpened = 40,
            ReplayFile = "repeticion-de-prueba-b.replay"
        }];
        vm.Refresh(matches, new Config { Profile = "sample" }, new SessionCounter(DateTime.UtcNow));
        vm.TeamImportSummary = "Datos ficticios · dos fuentes vinculadas de una misma partida.";
        vm.Footer = "VISTA DE PRUEBA · Datos ficticios · Sin capturas de pantalla ni acceso al historial real";
        window.DataContext = vm;
        ((ContentControl)window.FindName("SummaryPanel")).Content = new SummaryView { DataContext = vm };
        var summary = (SummaryView)((ContentControl)window.FindName("SummaryPanel")).Content;
        var liveToggle = (Button)summary.FindName("LiveToggle");
        if (summary.FindName("LiveSection") is not Border || liveToggle.Visibility != Visibility.Visible)
            throw new Exception("El botón de conteo debe estar visible sin expandir nada.");
        var toggleRequested = false;
        summary.ToggleLiveRequested += (_, _) => toggleRequested = true;
        liveToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        if (!toggleRequested || vm.VisualCaptureEnabled)
            throw new Exception("El resumen debe solicitar permiso, nunca activar capturas por su cuenta.");
        var picker = (ComboBox)window.FindName("ProfilePicker");
        picker.ItemsSource = new[] { "Cuenta de prueba", "Otra cuenta de prueba" };
        picker.SelectedIndex = 0;
        var routes = new RouteSuggestionsView(false);
        routes.MapControl.TileNetworkAllowed = false;
        routes.Planner.HighDetailEnabled = false;
        ((ContentControl)window.FindName("RoutesPanel")).Content = routes;
        ((ContentControl)window.FindName("ReportPanel")).Content = new PlayerReportView { DataContext = vm };
        ((ContentControl)window.FindName("StatisticsPanel")).Content = await PlayerStatisticsViewChecks.CreatePreview();
        var treasures = new TreasureOddsView();
        treasures.SetHistory(126, "Cuenta de prueba", "Período de prueba", 3);
        ((ContentControl)window.FindName("TreasuresPanel")).Content = treasures;
        var paths = new[] { "SummaryPanel", "StatisticsPanel", "HistoryPanel", "TeamPanel", "RoutesPanel", "TreasuresPanel", "ReportPanel", "SettingsPanel" };
        var navs = new[] { "NavSummary", "NavStatistics", "NavHistory", "NavTeam", "NavRoutes", "NavTreasures", "NavReport", "NavSettings" };
        var titles = new[] { "Resumen", "Estadísticas", "Historial", "Equipo", "Mapa y rutas", "Tesoros y probabilidades", "Informe", "Ajustes" };
        window.Show();
        try
        {
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            if (vm.TeamSelectedTotal != "≥ 112" || vm.TeamPlayers.Single(player => player.Name == "Marina de prueba").Count != "22"
                || vm.TeamPlayers.Single(player => player.Name == "Leo de prueba").Count != "≥ 17" || vm.Total != "126")
                throw new Exception("El panel no concilia las perspectivas o altera el total propio.");
            foreach (var width in new[] { 1180, 860 })
            {
                window.Width = width;
                window.Height = width == 860 ? 700 : 820;
                for (var index = 0; index < paths.Length; index++)
                {
                    foreach (var name in paths) ((FrameworkElement)window.FindName(name)).Visibility = name == paths[index] ? Visibility.Visible : Visibility.Collapsed;
                    ((TextBlock)window.FindName("PageTitle")).Text = titles[index];
                    ((TextBlock)window.FindName("PageSubtitle")).Text = "Vista de prueba · datos ficticios";
                    ((FrameworkElement)window.FindName("StatusPanel")).Visibility = paths[index] is "RoutesPanel" or "ReportPanel" or "StatisticsPanel" or "TreasuresPanel" ? Visibility.Collapsed : Visibility.Visible;
                    for (var n = 0; n < navs.Length; n++)
                    {
                        var button = (Button)window.FindName(navs[n]);
                        button.Background = n == index ? (Brush)app.Resources["Gold"] : Brushes.Transparent;
                        button.Foreground = (Brush)app.Resources[n == index ? "Ground" : "Ink"];
                    }
                    await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                    window.UpdateLayout();
                    if (((FrameworkElement)window.FindName(paths[index])).ActualWidth <= 0)
                        throw new Exception("Una pantalla no tiene espacio visible.");
                    if (output != null)
                    {
                        Directory.CreateDirectory(output);
                        var content = (FrameworkElement)window.Content;
                        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(content.ActualWidth), (int)Math.Ceiling(content.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                        bitmap.Render(content);
                        var png = new PngBitmapEncoder();
                        png.Frames.Add(BitmapFrame.Create(bitmap));
                        using var stream = File.Create(Path.Combine(output, $"{paths[index]}-{width}.png"));
                        png.Save(stream);
                        if (paths[index] == "SettingsPanel")
                        {
                            var settings = (ScrollViewer)window.FindName("SettingsPanel");
                            settings.ScrollToEnd();
                            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                            content.UpdateLayout();
                            var bottom = new RenderTargetBitmap((int)Math.Ceiling(content.ActualWidth), (int)Math.Ceiling(content.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                            bottom.Render(content);
                            var bottomPng = new PngBitmapEncoder();
                            bottomPng.Frames.Add(BitmapFrame.Create(bottom));
                            using var bottomFile = File.Create(Path.Combine(output, $"SettingsTools-{width}.png"));
                            bottomPng.Save(bottomFile);
                            settings.ScrollToTop();
                        }
                    }
                }
            }
            foreach (var dateName in new[] { "PeriodFrom", "PeriodThrough" })
            {
                var date = (DatePicker)window.FindName(dateName);
                if (!string.Equals(date.Language.IetfLanguageTag, "es-MX", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Una fecha no utiliza el idioma español.");
                date.SelectedDate = new DateTime(2026, 11, 15);
                date.UpdateLayout();
                var editor = (System.Windows.Controls.Primitives.DatePickerTextBox)date.Template.FindName("PART_TextBox", date);
                if (editor.Text != "15/11/2026")
                    throw new Exception("La fecha seleccionada no conserva su formato local.");
                date.SelectedDate = null;
            }
            vm.SelectedTeamMatch = vm.TeamMatches.Last();
            if (vm.TeamPlayers.Count != 1 || vm.TeamSelectedTotal != "—")
                throw new Exception("Una partida antigua sin datos de equipo inventa compañeros.");
            Console.WriteLine("20 comprobaciones del conjunto de pantallas correctas, con datos ficticios y sin red.");
        }
        finally { window.Close(); }
    }
}
