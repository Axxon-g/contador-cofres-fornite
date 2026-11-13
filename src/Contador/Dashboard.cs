using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ChestCounter;
using Contador.Engine;

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
public sealed class Dashboard : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
    public string Status { get; set; } = "Preparando el contador";
    public string StatusDetail { get; set; } = "Comprobando las repeticiones locales…";
    public Brush StatusColor { get; set; } = new SolidColorBrush(Color.FromRgb(241,202,118));
    public string Total { get; set; } = "0";
    public string Session { get; set; } = "0";
    public string ProfileLabel { get; set; } = "Tu cuenta se detectará al leer la primera partida.";
    public string LastMatch { get; set; } = "Última partida: todavía no hay datos.";
    public string Stats { get; set; } = "0 partidas · 0 tesoros";
    public string EmptyMessage { get; set; } = "Todavía no hay partidas. Activa las repeticiones y juega una partida.";
    public string Footer { get; set; } = "Lectura local · Sin conexión a tu cuenta de Epic";
    public string PeriodLabel { get; set; } = "Período: todo el historial";
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
        foreach (var row in rows.Where(r => (r.Date+" "+r.Mode).Contains(search, StringComparison.CurrentCultureIgnoreCase)))
            History.Add(row);
    }
    public void Refresh(TrackerData data, Config config, SessionCounter sessionCounter)
    {
        PeriodLabel = "Período: " + MatchPeriod.Describe(config);
        var matches = Profiles.View(data, config).Matches;
        Total = matches.Sum(m => (long)m.ChestsOpened).ToString("N0", L.Culture);
        var sessionMatches = sessionCounter.Matches(Profiles.View(data, config));
        Session = sessionMatches.Sum(m => (long)m.ChestsOpened).ToString("N0", L.Culture);
        SessionDetail = $"{sessionMatches.Count} partidas desde que abriste la app";
        ProfileLabel = Profiles.Current(data, config)?.Name ?? "Tu cuenta se detectará al leer la primera partida.";
        var ordered = matches.OrderByDescending(m => m.MatchStartUtc ?? m.AnalyzedAtUtc).ToList();
        LastMatch = ordered.Count > 0 ? $"Última partida: {ordered[0].ChestsOpened} cofres" : "Última partida: todavía no hay datos.";
        var avg = matches.Count == 0 ? 0 : matches.Average(m => m.ChestsOpened);
        Stats = $"{matches.Count} partidas · {matches.Sum(m => m.TreasureFinds.Count)} tesoros\nMedia {avg:0.0} · Récord {matches.Select(m => m.ChestsOpened).DefaultIfEmpty(0).Max()} cofres";
        rows = ordered.Select(MatchRow.From).ToList();
        Recent.Clear(); foreach (var row in rows.Take(4)) Recent.Add(row);
        EmptyMessage = matches.Count == 0 ? "No hay partidas para esta cuenta en el período elegido. Cambia las fechas en Ajustes o juega una partida con las repeticiones activadas." : "";
        Filter(); NotifyAll();
    }
    public void NotifyAll() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
}
