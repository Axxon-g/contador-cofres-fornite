using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ChestCounter;
using Contador.Engine;
using Contador.Live;

namespace Contador;

public sealed record MatchRow(string Id, DateTime DateSort, string Date, string Mode, int Chests, int Treasures, string Placement, string Detail)
{
    public static MatchRow From(MatchRecord m) => new(m.SessionId, m.MatchStartUtc ?? m.AnalyzedAtUtc,
        (m.MatchStartUtc ?? m.AnalyzedAtUtc).ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
        Pretty.Playlist(m.Playlist), m.ChestsOpened, m.TreasureFinds.Count,
        m.Placement is int p ? "#" + p : "—",
        $"{m.PlayerName ?? "Cuenta desconocida"} · {m.ChestsOpened} cofres · {m.TreasureFinds.Count} tesoros\n" +
        $"Repetición: {m.ReplayFile}" +
        (m.Chests.Count > 0 ? "\n" + string.Join(" · ", m.Chests.GroupBy(c => c.Kind).Select(g => $"{L.Name(g.Key)}: {g.Count()}")) : ""));
}
public sealed partial class Dashboard : INotifyPropertyChanged
{
    public sealed record SquadRow(string Name, string Chests, string Source);
    private bool visualCaptureEnabled;
    private LiveCountPhase visualPhase;
    public bool VisualCaptureEnabled
    {
        get => visualCaptureEnabled;
        set { if (visualCaptureEnabled == value) return; visualCaptureEnabled = value; NotifyVisualState(); }
    }
    public LiveCountPhase VisualPhase
    {
        get => visualPhase;
        set { if (visualPhase == value) return; visualPhase = value; NotifyVisualState(); }
    }
    public string VisualCaptureAction => VisualCaptureEnabled ? "Desactivar conteo" : "Activar conteo en tiempo real";
    public bool IsVisualCounting => VisualCaptureEnabled && VisualPhase == LiveCountPhase.Counting;
    public string VisualStateLabel => !VisualCaptureEnabled ? "Desactivado" : VisualPhase switch
    {
        LiveCountPhase.Counting => "Contando",
        LiveCountPhase.Paused => "En pausa",
        LiveCountPhase.Unavailable => "Necesita atención",
        _ => "Esperando partida"
    };
    public bool HasSquadRows => SquadRows.Count > 0;
    private void NotifyVisualState()
    {
        foreach (var name in new[] { nameof(VisualCaptureEnabled), nameof(VisualPhase),
            nameof(VisualCaptureAction), nameof(IsVisualCounting), nameof(VisualStateLabel) })
            Changed(name);
    }
    public string VisualCount { get; set; } = "—";
    public string VisualStatus { get; set; } = "Actívalo para ver una estimación de tus cofres mientras juegas.";
    public string VisualDiagnostic { get; set; } = "La lectura visual reconoce avisos XP/PE; no detecta la rotura de un cofre sin aviso.";
    public string SquadContext { get; set; } = "El equipo aparecerá al leer una partida.";
    public IReadOnlyList<SquadRow> SquadRows { get; set; } = Array.Empty<SquadRow>();
    public event PropertyChangedEventHandler? PropertyChanged;
    public void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
    public string Status { get; set; } = "Preparando el contador";
    public string StatusDetail { get; set; } = "Comprobando las repeticiones locales…";
    public Brush StatusColor { get; set; } = new SolidColorBrush(Color.FromRgb(241, 202, 118));
    public string Total { get; set; } = "0";
    public string Session { get; set; } = "0";
    public string ProfileLabel { get; set; } = "Tu cuenta se detectará al leer la primera partida.";
    public string LastMatch { get; set; } = "Última partida: todavía no hay datos.";
    public string Stats { get; set; } = "0 partidas · 0 tesoros";
    public string EmptyMessage { get; set; } = "Todavía no hay partidas. Activa las repeticiones y juega una partida.";
    public string Footer { get; set; } = "Lectura local · Sin conexión a tu cuenta de Epic";
    public string PeriodLabel { get; set; } = "Período: todo el historial";
    public bool ShowTeamStats { get; set; }
    public TeamAccounting TeamInfo { get; private set; } = new();
    public string TeamChestTotal => TeamInfo.CoveredMatches == 0 ? "—" : "≥ " + TeamInfo.TeamChests.ToString("N0", L.Culture);
    public string TeamChestSplit => TeamInfo.CoveredMatches == 0
        ? "Todavía no hay recuentos de compañeros para este período."
        : $"Tuyos en todo el período: {TeamInfo.OwnChests:N0} · Compañeros detectados: {TeamInfo.TeammateChests:N0}";
    public string TeamCoverage => $"Mismo período que el contador personal: {TeamInfo.Matches} partidas. "
        + $"Sólo {TeamInfo.CoveredMatches} tienen datos de compañeros; faltan {TeamInfo.Matches - TeamInfo.CoveredMatches}. "
        + "Es una suma mínima de registros, no el total real del equipo. Incluso en las partidas con datos puede faltar lo que ocurre lejos. "
        + (TeamInfo.CoveredMatches < TeamInfo.Matches
            ? "Si falta el archivo de repetición, no se puede recuperar ese dato del equipo." : "");
    public string TeamMembers => TeamInfo.MemberSummary;
    public string TeamEvidence => $"Perspectivas complementarias utilizadas: {TeamInfo.ReplaySourcesUsed}. "
        + $"Fuentes contradictorias sin añadir: {TeamInfo.ConflictingSources}. "
        + $"Interacciones de cofres sin autor, no asignadas: {TeamInfo.UnattributedInteractions}. "
        + "Importar más perspectivas amplía los datos registrados, sin garantizar que el juego grabara todo.";
    public string OwnChestRewards => $"{TeamInfo.OwnRecordedRewards:N0} recompensas registradas: "
        + $"{TeamInfo.OwnChestRewards + TeamInfo.OwnProbableChestRewards:N0} atribuidas a cofres";
    public string OtherOwnRewards => $"Origen directo observado en cofres: {TeamInfo.OwnChestRewards:N0}. "
        + $"Atribución estimada a cofres: {TeamInfo.OwnProbableChestRewards:N0}. "
        + $"Otras interacciones o sin origen: {TeamInfo.OwnOtherRewards:N0}. "
        + "La proximidad a una apertura no confirma que la recompensa proceda de ese cofre.";
    public string TeammateRewards => $"{TeamInfo.TeammateRewards:N0} "
        + "con origen directo observado"
        + $" · {TeamInfo.EstimatedTeammateRewards:N0} "
        + (TeamInfo.EstimatedTeammateRewards == 1 ? "estimada" : "estimadas");
    public PlayerReport Report { get; private set; } = new();
    public string ReportEmptyMessage => Report.Matches == 0
        ? "No hay partidas para esta cuenta y período. Elige otra cuenta o cambia las fechas en Ajustes." : "";
    private bool showTeamTreasures;
    public bool ShowTeamTreasures
    {
        get => showTeamTreasures;
        set
        {
            showTeamTreasures = value;
            Changed();
            Changed(nameof(ReportTreasures));
        }
    }
    public IEnumerable<ReportTreasure> ReportTreasures =>
        Report.Finds.Where(find => ShowTeamTreasures || !find.Team);
    public string MatchState { get; set; } = "Esperando partida";
    public string SessionDetail { get; set; } = "0 partidas desde que abriste la app";
    public bool IsBusy { get; set; }
    public bool Paused { get; set; }
    public bool CanEdit => !IsBusy;
    public bool CanScan => !IsBusy && !Paused;
    public string ScanLabel => IsBusy ? "Analizando…" : "Analizar ahora";
    public string PauseLabel => Paused ? "Reanudar detección" : "Pausar detección";
    private List<MatchRow> rows = [];
    public ObservableCollection<MatchRow> Recent { get; } = [];
    public ObservableCollection<MatchRow> History { get; } = [];
    private string search = "";
    public string Search { get => search; set { search = value; Changed(); Filter(); } }
    private MatchRow? selected;
    public MatchRow? SelectedMatch { get => selected; set { selected = value; Changed(); Changed(nameof(MatchDetail)); } }
    public string MatchDetail => SelectedMatch?.Detail ?? "Selecciona una partida para consultar sus detalles.";
    public void Filter()
    {
        History.Clear();
        foreach (var row in rows.Where(r => (r.Date + " " + r.Mode).Contains(search, StringComparison.CurrentCultureIgnoreCase)))
            History.Add(row);
    }
    public void Refresh(TrackerData data, Config config, SessionCounter sessionCounter)
    {
        PeriodLabel = "Período: " + MatchPeriod.Describe(config);
        var matches = Profiles.View(data, config).Matches;
        ShowTeamStats = config.ShowTeamStats;
        TeamInfo = TeamAccounting.Create(matches, data.TeamReplaySources.Concat(data.Matches));
        Report = PlayerReport.Create(data, config, sessionCounter, DateTime.Now);
        Total = matches.Sum(m => (long)m.ChestsOpened).ToString("N0", L.Culture);
        var sessionMatches = sessionCounter.Matches(Profiles.View(data, config));
        Session = sessionMatches.Sum(m => (long)m.ChestsOpened).ToString("N0", L.Culture);
        SessionDetail = $"{sessionMatches.Count} partidas desde que abriste la app";
        ProfileLabel = Profiles.Current(data, config)?.Name ?? "Tu cuenta se detectará al leer la primera partida.";
        var ordered = matches.OrderByDescending(m => m.MatchStartUtc ?? m.AnalyzedAtUtc).ToList();
        RefreshTeam(ordered, data.TeamReplaySources, data.Matches);
        LastMatch = ordered.Count > 0 ? $"Última partida: {ordered[0].ChestsOpened} cofres" : "Última partida: todavía no hay datos.";
        var avg = matches.Count == 0 ? 0 : matches.Average(m => m.ChestsOpened);
        Stats = $"{matches.Count} partidas · {matches.Sum(m => m.TreasureFinds.Count)} recompensas registradas\nMedia {avg:0.0} · Récord {matches.Select(m => m.ChestsOpened).DefaultIfEmpty(0).Max()} cofres";
        rows = ordered.Select(MatchRow.From).ToList();
        Recent.Clear(); foreach (var row in rows.Take(4)) Recent.Add(row);
        EmptyMessage = matches.Count == 0 ? "No hay partidas para esta cuenta en el período elegido. Cambia las fechas en Ajustes o juega una partida con las repeticiones activadas." : "";
        Filter(); NotifyAll();
    }
    public void NotifyAll() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
}
