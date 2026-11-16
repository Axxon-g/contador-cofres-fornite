using System.Net;
using System.Text;
using Contador.Engine.Statistics;

internal static class PlayerStatisticsChecks
{
    public const string Id = "10000000000000000000000000000001";
    private const string Example = """
    {"status":200,"version":1,"data":{"account":{"id":"10000000000000000000000000000001","name":"Jugador de prueba"},"battlePass":{"level":42},
    "stats":{"all":{"overall":{"wins":12,"matches":100,"kills":250,"kd":2.84,"winRate":12},"solo":{"wins":null,"matches":7},"duo":null,"trio":null}}}}
    """;

    public static void Run(Action<bool, string> check) => RunAsync(check).GetAwaiter().GetResult();
    private static async Task RunAsync(Action<bool, string> check)
    {
        var query = new PlayerStatisticsQuery(Id, true);
        var uri = PlayerStatisticsClient.BuildUri(query);
        check(uri.Host == "www.axxon.dev" && uri.Scheme == "https" && uri.AbsolutePath == "/api/fortnite-stats", "Stats utiliza el dominio definitivo sin depender de redirecciones");
        check(uri.Query.Contains("accountId=" + Id), "Stats enlaza por ID cuando existe");
        check(PlayerStatisticsClient.BuildUri(new("A & B?", AccountType: "psn")).Query.Contains("name=A%20%26%20B%3F"), "Stats escapa nombres y filtros");
        check(!new PlayerStatisticsQuery("x").IsValid && !new PlayerStatisticsQuery("Jugador\n").IsValid, "Stats valida nombres");
        var parsed = PlayerStatisticsParser.Parse(Encoding.UTF8.GetBytes(Example), query, DateTimeOffset.UtcNow);
        check(parsed.Modes.Count == 2 && parsed.Modes[0].Kills == 250, "Stats lee datos normalizados");
        check(parsed.Modes[1].Wins == null, "Stats conserva datos ausentes sin fabricar cero");
        var handler = new FakeHandler(() => Response(Example));
        var client = new PlayerStatisticsClient(new HttpClient(handler));
        await client.FetchAsync(query, default);
        await client.FetchAsync(query, default);
        check(handler.Calls == 1, "Stats usa caché para reducir consultas");
        check(!handler.AuthorizationSeen, "El contador no transmite claves ni cabeceras de autorización");
        check(handler.LastUri?.Host == "www.axxon.dev", "La consulta real del cliente usa el dominio definitivo");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        try { await client.FetchAsync(query, cancellation.Token); check(false, "Cancelación de estadísticas"); }
        catch (OperationCanceledException) { check(true, "La cancelación prevalece sobre la caché"); }
        foreach (var status in new[] { HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.ServiceUnavailable })
        {
            var error = new PlayerStatisticsClient(new HttpClient(new FakeHandler(() => Response("PRIVATE-ERROR", status))));
            try { await error.FetchAsync(query, default); check(false, "Error de estadísticas"); }
            catch (PlayerStatisticsException exception) { check(!exception.Message.Contains("PRIVATE-ERROR"), "El contador no reproduce errores privados del servidor"); }
        }
        var rateHandler = new FakeHandler(() => Response("{}", HttpStatusCode.TooManyRequests));
        var rateClient = new PlayerStatisticsClient(new HttpClient(rateHandler));
        for (var i = 0; i < 2; i++) try { await rateClient.FetchAsync(query, default); } catch (PlayerStatisticsException) { }
        check(rateHandler.Calls == 1, "Stats respeta los límites del servicio");
        var big = new PlayerStatisticsClient(new HttpClient(new FakeHandler(() => Response(new string('x', PlayerStatisticsClient.MaximumResponseBytes + 1)))));
        try { await big.FetchAsync(query, default); check(false, "Tamaño de estadísticas"); }
        catch (PlayerStatisticsException) { check(true, "Stats limita respuestas excesivas"); }
        var html = new PlayerStatisticsClient(new HttpClient(new FakeHandler(() => new(HttpStatusCode.NotFound) { Content = new StringContent("<html>Not deployed</html>") })));
        try { await html.FetchAsync(query, default); check(false, "Servicio no desplegado"); }
        catch (PlayerStatisticsException exception) { check(exception.Message.Contains("servicio"), "Una ruta sin desplegar no se confunde con un perfil no encontrado"); }
        try { PlayerStatisticsParser.Parse(Encoding.UTF8.GetBytes(Example), new("20000000000000000000000000000002", true), DateTimeOffset.UtcNow); check(false, "Identidad de estadísticas"); }
        catch (PlayerStatisticsException) { check(true, "Se rechaza una respuesta de otra cuenta"); }
    }
    private static HttpResponseMessage Response(string body, HttpStatusCode status = HttpStatusCode.OK) => new(status)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };
    private sealed class FakeHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public bool AuthorizationSeen { get; private set; }
        public Uri? LastUri { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            LastUri = request.RequestUri;
            AuthorizationSeen |= request.Headers.Contains("Authorization");
            return Task.FromResult(respond());
        }
    }
}
