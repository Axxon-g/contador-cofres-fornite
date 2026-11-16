using System.Net.Http;
using System.Text.Json;

namespace Contador.Engine.Updates;

public enum ReleaseCheckState { Current, Available, Unavailable }

public sealed record ReleaseCheckResult(ReleaseCheckState State, Version? Version = null);

/// <summary>Reads public release metadata. Never downloads or executes an update.</summary>
public sealed class ReleaseChecker(HttpClient http)
{
    public const string ReleasesPage = "https://github.com/Axxon-g/contador-cofres-fornite/releases/latest";
    private const string Endpoint = "https://api.github.com/repos/Axxon-g/contador-cofres-fornite/releases/latest";
    private const int MaximumBytes = 256 * 1024;

    public async Task<ReleaseCheckResult> CheckAsync(Version current, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(8));
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
            request.Headers.UserAgent.ParseAdd("Contador-ReleaseCheck/1.0");
            request.Headers.Accept.ParseAdd("application/vnd.github+json");
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength > MaximumBytes)
                return new(ReleaseCheckState.Unavailable);

            await using var source = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var body = new MemoryStream();
            var buffer = new byte[8192];
            int read;
            while ((read = await source.ReadAsync(buffer, timeout.Token)) > 0)
            {
                if (body.Length + read > MaximumBytes) return new(ReleaseCheckState.Unavailable);
                body.Write(buffer, 0, read);
            }
            using var json = JsonDocument.Parse(body.ToArray(), new JsonDocumentOptions { MaxDepth = 24 });
            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("draft", out var draft) || draft.ValueKind != JsonValueKind.False
                || !root.TryGetProperty("prerelease", out var preview) || preview.ValueKind != JsonValueKind.False
                || !root.TryGetProperty("tag_name", out var tag) || tag.ValueKind != JsonValueKind.String
                || !TryStableVersion(tag.GetString(), out var latest))
                return new(ReleaseCheckState.Unavailable);

            var installed = new Version(current.Major, current.Minor, Math.Max(0, current.Build));
            return latest > installed
                ? new(ReleaseCheckState.Available, latest)
                : new(ReleaseCheckState.Current);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(ReleaseCheckState.Unavailable);
        }
        catch (Exception error) when (error is HttpRequestException or IOException or JsonException)
        {
            return new(ReleaseCheckState.Unavailable);
        }
    }

    public static bool TryStableVersion(string? tag, out Version version)
    {
        version = new Version(0, 0, 0);
        if (string.IsNullOrWhiteSpace(tag) || tag.Length > 32) return false;
        var value = tag[0] is 'v' or 'V' ? tag[1..] : tag;
        var parts = value.Split('.');
        if (parts.Length is < 2 or > 3) return false;
        var values = new int[3];
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length == 0 || parts[i].Any(character => character is < '0' or > '9')
                || !int.TryParse(parts[i], out values[i])) return false;
        }
        version = new Version(values[0], values[1], values[2]);
        return true;
    }
}
