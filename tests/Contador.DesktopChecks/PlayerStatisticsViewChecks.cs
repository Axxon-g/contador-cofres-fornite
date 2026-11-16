using System.Windows;
using System.Windows.Controls;
using Contador;
using Contador.Engine.Statistics;
using Contador.Statistics;

namespace Contador.DesktopChecks;

internal static class PlayerStatisticsViewChecks
{
    private const string Id = "10000000000000000000000000000001";
    public static async Task<PlayerStatisticsView> CreatePreview()
    {
        var model = new PlayerStatisticsViewModel(new FakeClient()) { ManualName = "Cuenta de prueba" };
        model.SetProfile(Id, "Cuenta de prueba");
        await model.SearchAsync();
        return new PlayerStatisticsView(model);
    }
    public static async Task Run(Application app, string? output)
    {
        var count = 0;
        void Check(bool condition, string label) { if (!condition) throw new Exception(label); count++; }
        var client = new FakeClient();
        using var model = new PlayerStatisticsViewModel(client);
        Check(model.CanSearch && !model.UseDetectedAccount, "La búsqueda está disponible sin configurar claves");
        model.SetProfile(Id, "Cuenta de prueba");
        Check(client.Calls == 0, "Detectar la cuenta no transmite nada antes de buscar");
        model.ManualName = "Jugador";
        await model.SearchAsync();
        Check(model.HasResult && model.Wins == "12", "El nombre basta para consultar estadísticas");
        Check(model.KillsPerMatch == (2.5).ToString("N2") && model.PlayedTime == "25 h 0 min", "Las métricas derivadas usan el período consultado");
        Check(client.Query?.ByAccountId == false, "La consulta manual no necesita archivos locales");
        model.UseDetectedAccount = true;
        await model.SearchAsync();
        Check(client.Query?.ByAccountId == true && client.Query.Target == Id, "La cuenta local es una alternativa opcional");
        model.SetProfile("20000000000000000000000000000002", "Otra cuenta");
        await Task.Yield();
        Check(client.Query?.Target.StartsWith("2") == true, "El enlace sigue el perfil seleccionado");
        model.SelectedWindow = model.Windows[1];
        await model.SearchAsync();
        Check(client.Query?.TimeWindow == "season", "El filtro de temporada se aplica");
        client.Fail = true;
        await model.SearchAsync();
        Check(!model.HasResult && model.Message.Contains("privado") && model.KillsPerMatch == "—" && model.PlayedTime == "—", "Los errores no se presentan como estadísticas cero");
        model.Dispose();
        var before = client.Calls;
        await model.SearchAsync();
        Check(client.Calls == before, "Cerrar el panel impide consultas posteriores");
        Console.WriteLine($"{count} comprobaciones WPF de estadísticas sin clave correctas.");
    }
    private sealed class FakeClient : IPlayerStatisticsClient
    {
        public int Calls { get; private set; }
        public bool Fail { get; set; }
        public PlayerStatisticsQuery? Query { get; private set; }
        public Task<PlayerStatisticsSnapshot> FetchAsync(PlayerStatisticsQuery query, CancellationToken cancellationToken)
        {
            Calls++;
            Query = query;
            if (Fail) throw new PlayerStatisticsException("El perfil es privado. Prueba otro jugador.");
            return Task.FromResult(new PlayerStatisticsSnapshot("Cuenta de prueba", Id, query.TimeWindow, query.Input, 42,
                [new("General", 100, 12, 250, 2.84, 12, 1500), new("Solitario", 40, 5, 90, 2.57, 12.5, 500)], DateTimeOffset.UtcNow));
        }
    }
}
