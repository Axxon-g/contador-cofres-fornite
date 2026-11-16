using System.Net;
using System.Net.Http;

namespace Contador.Engine.Statistics;

public interface IPlayerStatisticsClient
{
    Task<PlayerStatisticsSnapshot> FetchAsync(PlayerStatisticsQuery query, CancellationToken cancellationToken);
}

/// <summary>Public AXXON relay. No provider key is present in the desktop application.</summary>
public sealed class PlayerStatisticsClient(HttpClient http) : IPlayerStatisticsClient
{
    public const string Endpoint = "https://www.axxon.dev/api/fortnite-stats";
    public const int MaximumResponseBytes = 256 * 1024;
    private readonly Dictionary<PlayerStatisticsQuery, PlayerStatisticsSnapshot> cache = new();
    private DateTimeOffset retryAfter;

    public async Task<PlayerStatisticsSnapshot> FetchAsync(PlayerStatisticsQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!query.IsValid) throw new PlayerStatisticsException("Revisa el nombre y los filtros de búsqueda.");
        var now = DateTimeOffset.UtcNow;
        if (cache.TryGetValue(query, out var previous) && now - previous.RetrievedAt < TimeSpan.FromMinutes(5)) return previous;
        if (now < retryAfter) throw new PlayerStatisticsException("El servicio pidió una pausa. Espera antes de volver a consultar.");
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(query));
        request.Headers.Accept.ParseAdd("application/json");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        try
        {
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var wait = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromMinutes(1);
                retryAfter = now.AddSeconds(Math.Clamp(wait.TotalSeconds, 60, 3600));
            }
            if (response.Content.Headers.ContentType?.MediaType != "application/json")
                throw new PlayerStatisticsException("El servicio de estadísticas todavía no está disponible. Inténtalo más tarde.");
            if (!response.IsSuccessStatusCode)
                throw new PlayerStatisticsException(response.StatusCode switch
                {
                    HttpStatusCode.Forbidden => "El perfil es privado o no está disponible. Si es tu cuenta, revisa Estadísticas públicas en Cuenta y privacidad del juego. También puedes buscar otro jugador y volver a consultar.",
                    HttpStatusCode.NotFound => "No encontramos estadísticas. Revisa el nombre y el tipo de cuenta.",
                    HttpStatusCode.TooManyRequests => "Hay muchas consultas. Espera antes de volver a intentarlo.",
                    HttpStatusCode.BadRequest => "Revisa el nombre y los filtros seleccionados.",
                    _ => "El servicio de estadísticas todavía no está disponible. Tus cofres locales siguen funcionando."
                });
            if (response.Content.Headers.ContentLength > MaximumResponseBytes)
                throw new PlayerStatisticsException("La respuesta supera el tamaño admitido.");
            await using var body = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var buffer = new MemoryStream();
            var bytes = new byte[8192];
            int length;
            while ((length = await body.ReadAsync(bytes, timeout.Token)) != 0)
            {
                if (buffer.Length + length > MaximumResponseBytes) throw new PlayerStatisticsException("La respuesta supera el tamaño admitido.");
                buffer.Write(bytes, 0, length);
            }
            cancellationToken.ThrowIfCancellationRequested();
            var result = PlayerStatisticsParser.Parse(buffer.ToArray(), query, now);
            if (cache.Count >= 16) cache.Clear();
            cache[query] = result;
            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new PlayerStatisticsException("La consulta tardó demasiado. Inténtalo de nuevo.");
        }
        catch (HttpRequestException)
        {
            throw new PlayerStatisticsException("No se pudo conectar con el servicio. Comprueba tu conexión.");
        }
    }

    public static Uri BuildUri(PlayerStatisticsQuery query)
    {
        if (!query.IsValid) throw new ArgumentException("Consulta no válida.", nameof(query));
        var target = query.ByAccountId ? "accountId=" + query.NormalizedTarget
            : "name=" + Uri.EscapeDataString(query.NormalizedTarget) + "&accountType=" + query.AccountType;
        return new Uri(Endpoint + "?" + target + "&timeWindow=" + query.TimeWindow + "&input=" + query.Input);
    }
}
