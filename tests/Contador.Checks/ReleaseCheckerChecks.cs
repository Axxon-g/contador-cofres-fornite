using System.Net;
using System.Net.Http;
using System.Text;
using Contador.Engine.Updates;

internal static class ReleaseCheckerChecks
{
    public static async Task Run(Action<bool, string> check)
    {
        check(ReleaseChecker.TryStableVersion("v2.2", out var shortVersion)
            && shortVersion == new Version(2, 2, 0), "La versión corta se normaliza");
        foreach (var tag in new[] { "2.3-beta", "2.3.0.1", "../2.3", "2.-1", "vv2.3", "2. 3", "99999999999.1" })
            check(!ReleaseChecker.TryStableVersion(tag, out _), "Rechaza etiquetas no estables: " + tag);

        var handler = new FakeHandler();
        using var http = new HttpClient(handler);
        var checker = new ReleaseChecker(http);
        var current = new Version(2, 2, 0);
        async Task<ReleaseCheckResult> Read(string json)
        {
            handler.Json = json;
            return await checker.CheckAsync(current, CancellationToken.None);
        }
        var newer = await Read("""{"tag_name":"v2.3.0","draft":false,"prerelease":false,"html_url":"https://untrusted.invalid"}""");
        check(newer.State == ReleaseCheckState.Available && newer.Version == new Version(2, 3, 0), "Detecta una versión posterior");
        check(handler.SafeRequest, "La consulta usa un destino fijo sin credenciales ni cuerpo");
        check(ReleaseChecker.ReleasesPage == "https://github.com/Axxon-g/contador-cofres-fornite/releases/latest", "El navegador no usa enlaces recibidos de la red");
        check((await Read("""{"tag_name":"2.2","draft":false,"prerelease":false}""")).State == ReleaseCheckState.Current, "No avisa con la misma versión");
        check((await Read("""{"tag_name":"1.3.2","draft":false,"prerelease":false}""")).State == ReleaseCheckState.Current, "No sugiere volver a una versión antigua");
        foreach (var json in new[]
        {
            """{"tag_name":"9.0","draft":true,"prerelease":false}""",
            """{"tag_name":"9.0","draft":false,"prerelease":true}""",
            """{"tag_name":"9.0"}""",
            """{"tag_name":42,"draft":false,"prerelease":false}""",
            "not json", "[]", new string(' ', 300_000)
        })
            check((await Read(json)).State == ReleaseCheckState.Unavailable, "Los metadatos inválidos no ofrecen una actualización");
        foreach (var status in new[] { HttpStatusCode.MovedPermanently, HttpStatusCode.NotFound, HttpStatusCode.TooManyRequests })
        {
            handler.Status = status;
            check((await checker.CheckAsync(current, CancellationToken.None)).State == ReleaseCheckState.Unavailable, "El fallo HTTP no se presenta como versión actual");
        }
        handler.Status = HttpStatusCode.OK;
        handler.Timeout = true;
        check((await checker.CheckAsync(current, CancellationToken.None)).State == ReleaseCheckState.Unavailable, "El tiempo límite no interrumpe la app");
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        try
        {
            await checker.CheckAsync(current, cancelled.Token);
            check(false, "Debe respetar la cancelación al cerrar");
        }
        catch (OperationCanceledException) { check(true, "Respeta la cancelación al cerrar"); }
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        public string Json { get; set; } = "{}";
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        public bool Timeout { get; set; }
        public bool SafeRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Timeout) throw new TaskCanceledException();
            SafeRequest = request.Method == HttpMethod.Get && request.Content == null
                && request.Headers.Authorization == null
                && request.RequestUri?.AbsoluteUri == "https://api.github.com/repos/Axxon-g/contador-cofres-fornite/releases/latest";
            return Task.FromResult(new HttpResponseMessage(Status)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });
        }
    }
}
