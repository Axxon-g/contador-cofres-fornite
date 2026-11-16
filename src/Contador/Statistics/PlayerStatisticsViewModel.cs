using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Contador.Engine.Statistics;

namespace Contador.Statistics;

public sealed class PlayerStatisticsViewModel : INotifyPropertyChanged, IDisposable
{
    public sealed record Option(string Key, string Label);
    public sealed record ModeRow(string Mode, string Matches, string Wins, string Kills, string Kd, string WinRate);
    private static readonly HttpClient SharedHttp = new(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false });
    private readonly IPlayerStatisticsClient client;
    private CancellationTokenSource? request;
    private long requestVersion;
    private bool useDetectedAccount;
    private bool disposed;
    private bool hasSearchedProfile;
    private string? detectedId;
    private string detectedName = "";
    private Option selectedWindow;
    private Option selectedInput;
    private Option selectedAccountType;

    public PlayerStatisticsViewModel(IPlayerStatisticsClient? client = null)
    {
        this.client = client ?? new PlayerStatisticsClient(SharedHttp);
        selectedWindow = Windows[0];
        selectedInput = Inputs[0];
        selectedAccountType = AccountTypes[0];
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public IReadOnlyList<Option> Windows { get; } = [new("lifetime", "Todo el historial"), new("season", "Temporada actual")];
    public IReadOnlyList<Option> Inputs { get; } = [new("all", "Todos los controles"), new("keyboardMouse", "Teclado y ratón"), new("gamepad", "Mando"), new("touch", "Táctil")];
    public IReadOnlyList<Option> AccountTypes { get; } = [new("epic", "Epic Games"), new("psn", "PlayStation"), new("xbl", "Xbox")];
    public Option SelectedWindow { get => selectedWindow; set => SetFilter(ref selectedWindow, value); }
    public Option SelectedInput { get => selectedInput; set => SetFilter(ref selectedInput, value); }
    public Option SelectedAccountType { get => selectedAccountType; set => SetFilter(ref selectedAccountType, value); }
    public string ManualName { get; set; } = "";
    public bool UseDetectedAccount
    {
        get => useDetectedAccount;
        set
        {
            if (useDetectedAccount == value) return;
            useDetectedAccount = value;
            hasSearchedProfile = false;
            Reset();
        }
    }
    public bool ManualEnabled => !UseDetectedAccount;
    public string DetectedProfile => string.IsNullOrWhiteSpace(detectedName) ? "Sin cuenta local detectada" : detectedName;
    public bool IsBusy { get; private set; }
    public bool CanSearch => !IsBusy;
    public bool HasResult { get; private set; }
    public string Message { get; private set; } = "Escribe el nombre de un jugador para consultar sus estadísticas públicas.";
    public string AccountName { get; private set; } = "";
    public string ResultContext { get; private set; } = "";
    public string Updated { get; private set; } = "";
    public string Wins { get; private set; } = "—";
    public string Matches { get; private set; } = "—";
    public string Kills { get; private set; } = "—";
    public string Kd { get; private set; } = "—";
    public string WinRate { get; private set; } = "—";
    public string KillsPerMatch { get; private set; } = "—";
    public string PlayedTime { get; private set; } = "—";
    public string BattlePassLevel { get; private set; } = "—";
    public IReadOnlyList<ModeRow> Modes { get; private set; } = [];

    public void SetProfile(string? epicId, string? name)
    {
        if (disposed) return;
        var id = NormalizeId(epicId);
        name ??= "";
        if (detectedId == id && detectedName == name) return;
        detectedId = id;
        detectedName = name;
        Changed(nameof(DetectedProfile));
        if (!UseDetectedAccount) return;
        Reset();
        if (hasSearchedProfile) _ = SearchAsync();
    }

    private void SetFilter(ref Option current, Option value, [CallerMemberName] string? name = null)
    {
        if (value == null || current == value) return;
        current = value;
        Reset();
        Changed(name);
    }

    private void Reset()
    {
        Cancel();
        ClearResult();
        Message = "Consulta el perfil con los filtros seleccionados.";
        Notify();
    }

    public async Task SearchAsync()
    {
        if (disposed) return;
        Reset();
        var query = new PlayerStatisticsQuery(UseDetectedAccount ? detectedId ?? detectedName : ManualName,
            UseDetectedAccount && detectedId != null, UseDetectedAccount ? "epic" : SelectedAccountType.Key,
            SelectedWindow.Key, SelectedInput.Key);
        if (!query.IsValid)
        {
            Message = UseDetectedAccount ? "No hay un perfil utilizable. Desmarca Usar cuenta seleccionada y escribe un nombre."
                : "Escribe un nombre válido y selecciona Epic Games, PlayStation o Xbox.";
            Notify();
            return;
        }
        hasSearchedProfile = UseDetectedAccount;
        using var cancellation = new CancellationTokenSource();
        request = cancellation;
        var version = ++requestVersion;
        IsBusy = true;
        Message = "Consultando estadísticas públicas…";
        Notify();
        try
        {
            var snapshot = await client.FetchAsync(query, cancellation.Token);
            if (version != requestVersion || cancellation.IsCancellationRequested) return;
            Apply(snapshot);
            Message = snapshot.Modes.Count == 0 ? "Sin datos para estos filtros. — no significa cero."
                : "Datos del servicio de AXXON y Fortnite-API. El historial de cofres se mantiene separado.";
        }
        catch (OperationCanceledException) { }
        catch (PlayerStatisticsException exception) { if (version == requestVersion) Message = exception.Message; }
        catch (Exception) { if (version == requestVersion) Message = "No se pudo consultar el perfil. Inténtalo más tarde."; }
        finally
        {
            if (version == requestVersion) { request = null; IsBusy = false; Notify(); }
        }
    }
    private void Apply(PlayerStatisticsSnapshot snapshot)
    {
        HasResult = true;
        AccountName = snapshot.AccountName;
        ResultContext = (snapshot.TimeWindow == "season" ? "Temporada actual" : "Historial del proveedor") + " · "
            + Inputs.First(input => input.Key == snapshot.Input).Label;
        Updated = $"Consulta: {snapshot.RetrievedAt.ToLocalTime():dd/MM/yyyy HH:mm} · caché de hasta 5 minutos";
        var overall = snapshot.Modes.FirstOrDefault(mode => mode.Mode == "General");
        Wins = Count(overall?.Wins);
        Matches = Count(overall?.Matches);
        Kills = Count(overall?.Kills);
        Kd = overall?.Kd?.ToString("N2") ?? "—";
        WinRate = overall?.WinRate?.ToString("N1") is string rate ? rate + " %" : "—";
        KillsPerMatch = overall?.Kills is long kills && overall.Matches is > 0
            ? ((double)kills / overall.Matches.Value).ToString("N2") : "—";
        PlayedTime = overall?.MinutesPlayed is long minutes
            ? $"{minutes / 60:N0} h {minutes % 60} min" : "—";
        BattlePassLevel = Count(snapshot.BattlePassLevel);
        Modes = snapshot.Modes.Select(mode => new ModeRow(mode.Mode, Count(mode.Matches), Count(mode.Wins), Count(mode.Kills),
            mode.Kd?.ToString("N2") ?? "—", mode.WinRate?.ToString("N1") is string value ? value + " %" : "—")).ToList();
    }

    private void ClearResult()
    {
        HasResult = false;
        Modes = [];
        AccountName = ResultContext = Updated = "";
        Wins = Matches = Kills = Kd = WinRate = BattlePassLevel = KillsPerMatch = PlayedTime = "—";
    }

    public void Cancel(bool showMessage = false)
    {
        if (showMessage) Message = "Consulta cancelada.";
        requestVersion++;
        request?.Cancel();
        request = null;
        IsBusy = false;
        Notify();
    }

    public void Dispose()
    {
        disposed = true;
        Cancel();
    }
    private static string Count(long? value) => value?.ToString("N0") ?? "—";
    private static string? NormalizeId(string? id) => Guid.TryParse(id, out var value) ? value.ToString("N") : null;
    private void Notify() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    private void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
