using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ChestCounter;
using Microsoft.Win32;
using Contador.Engine;

namespace Contador;

public partial class MainWindow : Window
{
    private readonly Dashboard vm = new();
    private readonly ProfilePickerController profilePicker;
    private readonly Store store;
    private readonly Config config;
    private readonly TrackerData data;
    private readonly Tracker tracker;
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly SessionCounter sessionCounter = new(DateTime.UtcNow);
    private readonly DispatcherTimer presenceTimer = new() { Interval = TimeSpan.FromSeconds(2) };
    private bool checkingPresence, recording;
    private bool closing;
    public bool IsClosing => closing;
    private readonly WindowCloseController closeController;
    public Func<WindowCloseChoice>? CloseChoiceRequested { get; set; }
    public Action? BackgroundRequested { get; set; }
    public void RequestExit() => closeController.RequestExit();
    private bool watcherStarted;
    private readonly OverlayPublisher overlayPublisher;
    private string importNotice = "";
    private bool reportWritten;
    private bool scanning;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = vm;
        ReportPanel.OpenHtmlRequested += async (_, _) => await OpenHtmlReportAsync();
        Console.SetOut(TextWriter.Null);
        L.Set("es");
        // Separate storage: never migrate or overwrite the original application's data.
        store = new Store(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ContadorModern"));
        ErrorLog.FilePath = store.ErrorLogPath;
        config = store.LoadConfig() ?? new Config
        {
            Language = "es",
            UpdateMode = "off",
            OpenReportOnStart = false,
            ShowOverlay = false,
            OverlayLocked = true,
            LiveUpdate = false,
            DemosFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FortniteGame", "Saved", "Demos")
        };
        config.Language = "es";
        config.UpdateMode = "off";
        // Counts completed matches; no speculative live total.
        config.LiveUpdate = false;
        data = store.LoadData();
        sessionCounter.ExcludeHistory(data);
        ReportWriter.SessionFilter = sessionCounter.Includes;
        ReportWriter.ReportBrandImage = () => "data:image/png;base64," + Convert.ToBase64String(TokenIcon.Frame(128));
        overlayPublisher = new OverlayPublisher(
            sessionCounter.Overlay(Profiles.View(data, config), false),
            model =>
            {
                if (watcherStarted) NativeOverlay.Update(model);
                ReportWriter.WriteOverlay(store, model);
            });
        ReportWriter.OverlayWriter = (target, _) => ObsHud.Write(target, overlayPublisher.Current);
        tracker = new Tracker(store, config, data) { WriteReports = false, CompactAfterAnalysis = false };
        tracker.AnalysisStarting = () => Dispatcher.Invoke(ShowAnalysisStatus);
        DemosPath.Text = config.DemosFolder;
        OverlayCheck.IsChecked = config.ShowOverlay;
        OverlayScale.Value = config.OverlaySize;
        PeriodFrom.Language = PeriodThrough.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
        PeriodFrom.SelectedDate = config.MatchDateFrom;
        PeriodThrough.SelectedDate = config.MatchDateThrough;
        profilePicker = new ProfilePickerController(ProfilePicker, key => SaveAsync(() => config.Profile = key));
        Refresh();
        Navigate("summary");
        Loaded += async (_, _) =>
        {
            presenceTimer.Start();
            await CheckPresenceAsync();
            await gate.WaitAsync();
            vm.IsBusy = true; vm.NotifyAll();
            try
            {
                vm.Status = "Recuperando tu historial";
                vm.StatusDetail = "Buscando partidas de ChestCounter y ChestTracker en este equipo…";
                vm.NotifyAll();
                var imported = await Task.Run(() => HistoryImporter.Import(store, data,
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));
                importNotice = string.Join(" ", new[] { imported.Summary }.Concat(imported.Warnings).Where(s => s.Length > 0));
                sessionCounter.ExcludeHistory(data);
                Refresh();
                await Task.Run(() => tracker.UpgradeIfNeeded());
                store.SaveConfig(config);
            }
            catch (Exception ex) { importNotice = "No se pudo recuperar el historial anterior. Consulta el registro en Ajustes."; ShowError(ex); }
            finally { vm.IsBusy = false; gate.Release(); vm.NotifyAll(); }
            if (!closing) { ApplyOverlay(); await ScanAsync(); if (!closing) timer.Start(); }
        };
        timer.Tick += async (_, _) => await ScanAsync();
        presenceTimer.Tick += async (_, _) => await CheckPresenceAsync();
        closeController = new WindowCloseController(this, SaveBeforeCloseAsync, BeginClose,
            ex => MessageBox.Show(this, ex.Message, "No se pudieron guardar los datos"),
            () => CloseChoiceRequested?.Invoke() ?? WindowCloseChoice.Exit,
            () => { if (BackgroundRequested != null) BackgroundRequested(); else Hide(); });
    }

    private void ShowAnalysisStatus()
    {
        if (closing) return;
        vm.IsBusy = true;
        vm.Status = "Analizando repeticiones";
        vm.StatusDetail = "Puedes consultar el historial mientras termina el análisis.";
        vm.NotifyAll();
    }

    private async Task ScanAsync(bool userRequested = false)
    {
        if (closing || scanning || vm.IsBusy || vm.Paused || !await gate.WaitAsync(0)) return;
        scanning = true;
        var oldStatus = (vm.Status, vm.StatusDetail, vm.StatusColor, vm.Footer);
        try
        {
            if (userRequested) ShowAnalysisStatus();
            var scan = await Task.Run(() =>
            {
                var before = data.Matches.Count;
                var added = tracker.Scan();
                return (Added: added, Changed: added > 0 || data.Matches.Count != before);
            });
            var count = scan.Added;
            if (scan.Changed) Refresh();
            var exists = Directory.Exists(config.DemosFolder);
            var errors = data.ProcessedFiles.Values.Count(v => v.StartsWith("error:", StringComparison.Ordinal));
            vm.Status = !exists ? "No se encontró la carpeta de repeticiones"
                : count > 0 ? "Partida actualizada"
                : tracker.IsFinishing ? "La partida terminó. Preparando el recuento"
                : recording ? "Partida en curso"
                : errors > 0 ? "Hay repeticiones que no se pudieron leer"
                : "Listo para tu próxima partida";
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(
                exists && errors == 0 ? "#A9D5A5" : "#F1CA76");
            if (vm.StatusColor is not System.Windows.Media.SolidColorBrush current || current.Color != color)
                vm.StatusColor = new System.Windows.Media.SolidColorBrush(color);
            vm.StatusDetail = !exists ? "Activa Grabar repeticiones en Fortnite o elige la carpeta correcta en Ajustes."
                : errors > 0 ? $"{errors} archivos con error. Consulta el registro en Ajustes; el lector puede necesitar una actualización."
                : recording ? "Los cofres se sumarán cuando Fortnite finalice la repetición."
                : "Detección cada segundo. Los cofres se actualizan al finalizar la repetición.";
            if (scan.Changed || userRequested || !reportWritten)
                vm.Footer = count > 0 ? $"{count} partida(s) añadida(s) · Última revisión {DateTime.Now:HH:mm:ss}"
                    : $"Última revisión {DateTime.Now:HH:mm:ss} · Historial guardado localmente";
            if (importNotice.Length > 0) vm.Footer = importNotice;
            UpdateOverlay();
            if (scan.Changed || !reportWritten)
            {
                await Task.Run(() => ReportWriter.WriteAll(store, config, data));
                reportWritten = true;
            }
            if (vm.Paused) { vm.Status = "Detección en pausa"; vm.StatusDetail = "El análisis anterior terminó. Reanuda la detección para revisar las siguientes repeticiones."; }
        }
        catch (Exception ex) { ShowError(ex); }
        finally
        {
            var wasBusy = vm.IsBusy;
            vm.IsBusy = false;
            scanning = false;
            gate.Release();
            if (wasBusy || oldStatus != (vm.Status, vm.StatusDetail, vm.StatusColor, vm.Footer)) vm.NotifyAll();
        }
    }

    private void Refresh()
    {
        vm.Refresh(data, config, sessionCounter);
        profilePicker.Refresh(data, config);
        RefreshLiveMatch();
    }
    private void ShowError(Exception ex)
    {
        ErrorLog.Write("Contador desktop", ex);
        vm.Status = "No se pudo completar la operación";
        vm.StatusDetail = ex.Message + " · Consulta el registro de errores en Ajustes.";
        vm.NotifyAll();
    }
    private async Task SaveAsync(Action change)
    {
        if (closing) return;
        await gate.WaitAsync();
        try { change(); store.SaveConfig(config); Refresh(); UpdateOverlay(); reportWritten = false; }
        catch (Exception ex) { ShowError(ex); }
        finally { gate.Release(); }
    }
    private void ApplyOverlay()
    {
        if (!config.ShowOverlay) { NativeOverlay.Stop(); watcherStarted = false; return; }
        NativeOverlay.SetStyle("hud");
        NativeOverlay.SetAppearance(config.OverlaySize, config.OverlayOpacity);
        if (!watcherStarted)
        {
            NativeOverlay.Start(config.OverlayX, config.OverlayY, config.OverlayLocked,
                (x, y) => Dispatcher.BeginInvoke(new Action(async () => await SaveAsync(() => { config.OverlayX = x; config.OverlayY = y; }))),
                locked => Dispatcher.BeginInvoke(new Action(async () => await SaveAsync(() => config.OverlayLocked = locked))));
            watcherStarted = true;
        }
        UpdateOverlay(force: true);
    }
    private void UpdateOverlay(bool force = false)
    {
        // Keep the session count consistent with the dashboard, including imports.
        var model = sessionCounter.Overlay(Profiles.View(data, config), recording);
        if (recording && visualSession != null)
            model = model with { VisualEstimate = "≈ " + visualEstimate };
        overlayPublisher.Publish(model, force);
    }
    private async Task CheckPresenceAsync()
    {
        if (closing || checkingPresence) return;
        checkingPresence = true;
        try
        {
            var folder = config.DemosFolder;
            var activeReplay = await Task.Run(() => MatchPresence.FindRecording(folder, DateTime.UtcNow));
            recording = activeReplay != null;
            if (closing) return;
            SetVisualRecording(activeReplay);
            RefreshLiveMatch();
            var state = recording ? "EN PARTIDA · Cofres al finalizar" : "Esperando partida";
            if (vm.MatchState != state)
            {
                vm.MatchState = state;
                vm.Changed(nameof(Dashboard.MatchState));
            }
            // The lightweight presence probe stays responsive while a replay is being decoded.
            // Data snapshots for overlay totals are only built outside the scanner's mutation window.
            if (!scanning && !vm.IsBusy) UpdateOverlay();
            else overlayPublisher.UpdatePresence(recording);
        }
        finally { checkingPresence = false; }
    }
    private void Navigate(string page)
    {
        foreach (var (button, key) in new[] { (NavSummary, "summary"), (NavHistory, "history"), (NavTeam, "team"), (NavRoutes, "routes"), (NavReport, "report"), (NavSettings, "settings") })
        {
            button.Background = page == key ? (System.Windows.Media.Brush)FindResource("Gold") : System.Windows.Media.Brushes.Transparent;
            button.Foreground = (System.Windows.Media.Brush)FindResource(page == key ? "Ground" : "Ink");
        }
        SummaryPanel.Visibility = page == "summary" ? Visibility.Visible : Visibility.Collapsed;
        HistoryPanel.Visibility = page == "history" ? Visibility.Visible : Visibility.Collapsed;
        TeamPanel.Visibility = page == "team" ? Visibility.Visible : Visibility.Collapsed;
        RoutesPanel.Visibility = page == "routes" ? Visibility.Visible : Visibility.Collapsed;
        ReportPanel.Visibility = page == "report" ? Visibility.Visible : Visibility.Collapsed;
        StatusPanel.Visibility = page is "routes" or "report" ? Visibility.Collapsed : Visibility.Visible;
        SettingsPanel.Visibility = page == "settings" ? Visibility.Visible : Visibility.Collapsed;
        PageTitle.Text = page switch
        {
            "history" => "Historial",
            "team" => "Equipo",
            "routes" => "Mapa y rutas",
            "report" => "Informe",
            "settings" => "Ajustes",
            _ => "Resumen"
        };
        PageSubtitle.Text = page switch
        {
            "history" => "Consulta tus partidas y exporta el registro.",
            "team" => "Reúne las perspectivas de una misma partida.",
            "routes" => "Explora el mapa y prepara tu próximo recorrido.",
            "report" => "Tu progreso, tendencias y hallazgos.",
            "settings" => "Personaliza fechas, repeticiones y overlay.",
            _ => "Tu actividad, en un vistazo."
        };
    }
    private void TeamPageClick(object sender, RoutedEventArgs e) => Navigate("team");
    private void SummaryClick(object sender, RoutedEventArgs e) => Navigate("summary");
    private void HistoryClick(object sender, RoutedEventArgs e) => Navigate("history");
    private void RoutesClick(object sender, RoutedEventArgs e) => Navigate("routes");
    private void ReportPageClick(object sender, RoutedEventArgs e) => Navigate("report");
    private void SettingsClick(object sender, RoutedEventArgs e) => Navigate("settings");
    private async void ScanClick(object sender, RoutedEventArgs e) => await ScanAsync(userRequested: true);
    private async void TeamStatsClick(object sender, RoutedEventArgs e)
    {
        var enabled = (sender as CheckBox)?.IsChecked == true;
        await SaveAsync(() => config.ShowTeamStats = enabled);
    }
    private void PauseClick(object sender, RoutedEventArgs e)
    {
        vm.Paused = !vm.Paused;
        RefreshLiveMatch();
        if (!vm.IsBusy)
        {
            vm.Status = vm.Paused ? "Detección en pausa" : "Detección reanudada";
            vm.StatusDetail = vm.Paused ? "Reanuda la detección para analizar las siguientes repeticiones." : "La próxima revisión comenzará en unos segundos.";
        }
        vm.NotifyAll();
    }
    private void PeriodDateError(object? sender, DatePickerDateValidationErrorEventArgs e)
    {
        e.ThrowException = false;
        PeriodError.Text = "Fecha no válida. Elige el día en el calendario o corrige el texto.";
    }
    private async void ApplyPeriodClick(object sender, RoutedEventArgs e)
    {
        static bool Read(DatePicker picker, out DateTime? date)
        {
            date = null;
            if (string.IsNullOrWhiteSpace(picker.Text)) return true;
            if (!DateTime.TryParse(picker.Text, System.Globalization.CultureInfo.CurrentCulture,
                System.Globalization.DateTimeStyles.None, out var value)) return false;
            date = value.Date;
            return true;
        }
        if (!Read(PeriodFrom, out var from) || !Read(PeriodThrough, out var through))
        {
            PeriodError.Text = "Fecha no válida. Elige el día en el calendario o corrige el texto.";
            return;
        }
        await SetPeriodAsync(from, through);
    }
    private async Task SetPeriodAsync(DateTime? from, DateTime? through)
    {
        if (!MatchPeriod.IsValid(from, through))
        {
            PeriodError.Text = "La fecha inicial debe ser anterior o igual a la fecha final.";
            return;
        }
        PeriodError.Text = "Aplicando fechas…";
        await SaveAsync(() =>
        {
            config.MatchDateFrom = from?.Date;
            config.MatchDateThrough = through?.Date;
            PeriodFrom.SelectedDate = from?.Date;
            PeriodThrough.SelectedDate = through?.Date;
        });
        PeriodError.Text = "";
    }
    private async void EventPeriodClick(object sender, RoutedEventArgs e) =>
        await SetPeriodAsync(new DateTime(2026, 11, 1), null);
    private async void ClearPeriodClick(object sender, RoutedEventArgs e) => await SetPeriodAsync(null, null);
    private async void FolderClick(object sender, RoutedEventArgs e)
    {
        var picker = new OpenFolderDialog { Title = "Carpeta de repeticiones de Fortnite", Multiselect = false };
        if (picker.ShowDialog(this) == true)
        {
            await SaveAsync(() => { config.DemosFolder = picker.FolderName; DemosPath.Text = config.DemosFolder; });
            await ScanAsync();
        }
    }

    private async void OverlayClick(object sender, RoutedEventArgs e)
    {
        await SaveAsync(() => config.ShowOverlay = OverlayCheck.IsChecked == true);
        ApplyOverlay();
    }
    private void ToggleOverlayClick(object sender, RoutedEventArgs e) { if (watcherStarted) NativeOverlay.Toggle(); else MessageBox.Show(this, "Activa primero el overlay.", "Overlay"); }
    private void LockOverlayClick(object sender, RoutedEventArgs e) { if (watcherStarted) NativeOverlay.ToggleLock(); }
    private async void ScaleClick(object sender, RoutedEventArgs e)
    {
        await SaveAsync(() => config.OverlaySize = (int)OverlayScale.Value);
        NativeOverlay.SetAppearance(config.OverlaySize, config.OverlayOpacity);
    }
    private void OpenPath(string path)
    {
        try
        {
            if (!File.Exists(path) && !Directory.Exists(path)) { MessageBox.Show(this, "Todavía no existe este archivo o carpeta.", "Contador"); return; }
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex) { ShowError(ex); }
    }
    private void OpenDemosClick(object sender, RoutedEventArgs e) => OpenPath(config.DemosFolder);
    private void OpenDataClick(object sender, RoutedEventArgs e) => OpenPath(store.Folder);
    private async Task OpenHtmlReportAsync()
    {
        if (closing) return;
        await gate.WaitAsync();
        try
        {
            if (closing) return;
            await Task.Run(() => ReportWriter.WriteAll(store, config, data));
            reportWritten = true;
            OpenPath(store.ReportPath);
        }
        catch (Exception ex) { ShowError(ex); }
        finally { gate.Release(); }
    }
    private void ErrorsClick(object sender, RoutedEventArgs e) => OpenPath(store.ErrorLogPath);
    private void ExportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "partidas.csv" };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            // Escape formula prefixes as well as CSV separators in player-controlled values.
            var lines = new List<string> { "Fecha,Modo,Cofres,Tesoros,Posicion" };
            lines.AddRange(vm.History.Select(r => string.Join(",", new object?[]
            {
                r.Date, r.Mode, r.Chests, r.Treasures, r.Placement
            }.Select(CsvText.Cell))));
            File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(true));
            vm.Footer = $"Exportación guardada: {dialog.FileName}"; vm.NotifyAll();
        }
        catch (Exception ex) { ShowError(ex); }
    }
    private void BeginClose()
    {
        closing = true; timer.Stop(); presenceTimer.Stop();
        StopVisualCapture();
        vm.Status = "Guardando antes de salir";
        vm.StatusDetail = vm.IsBusy ? "Esperando a que termine el análisis actual…" : "Cerrando el contador…";
        vm.NotifyAll();
    }
    private async Task SaveBeforeCloseAsync()
    {
        await gate.WaitAsync();
        try { NativeOverlay.Stop(); store.SaveConfig(config); store.SaveData(data); }
        finally { gate.Release(); }
    }
}
