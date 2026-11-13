using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;
using Contador.Engine;

namespace Contador;

/// <summary>Optional public terrain only. No game, account or history data is sent.</summary>
public sealed class MapTileCache : IDisposable
{
    private static readonly HttpClient Http = CreateClient();
    private readonly ConcurrentDictionary<MapTile, (BitmapSource Image, long Used)> memory = new();
    private readonly ConcurrentDictionary<MapTile, byte> pending = new();
    private readonly ConcurrentDictionary<MapTile, DateTime> failed = new();
    private readonly SemaphoreSlim connections = new(4);
    private readonly CancellationTokenSource lifetime = new();
    private readonly Action changed;
    private readonly string folder;
    private readonly bool allowNetwork;
    private HashSet<MapTile> wanted = [];
    private long clock;
    private int disposed;
    private string error = "";
    private bool diskFailed;
    public string Status => error.Length > 0 && Volatile.Read(ref wanted).Any(tile => failed.ContainsKey(tile)) ? error
        : pending.Count > 0 ? "Terreno HD: cargando piezas · Fortnite.GG"
        : diskFailed ? "Terreno HD disponible; no se pudo guardar la caché."
        : "Terreno HD · Fortnite.GG · caché local";

    public MapTileCache(Action changed, string? folder = null, bool allowNetwork = true)
    {
        this.changed = changed;
        this.folder = folder ?? Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "ContadorModern", "MapTiles", "43");
        this.allowNetwork = allowNetwork;
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(8)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Contador/" + AppVersion.Full);
        return client;
    }

    public void Request(IReadOnlyList<MapTile> tiles)
    {
        if (Volatile.Read(ref disposed) != 0) return;
        Volatile.Write(ref wanted, tiles.ToHashSet());
        foreach (var tile in tiles)
        {
            if (!tile.IsValid || memory.ContainsKey(tile) || pending.Count >= 48) continue;
            if (failed.TryGetValue(tile, out var retry) && retry > DateTime.UtcNow) continue;
            if (pending.TryAdd(tile, 0)) _ = Load(tile);
        }
    }

    public BitmapSource? Get(MapTile tile)
    {
        if (!memory.TryGetValue(tile, out var entry)) return null;
        memory[tile] = (entry.Image, Interlocked.Increment(ref clock));
        return entry.Image;
    }

    private async Task Load(MapTile tile)
    {
        var entered = false;
        try
        {
            await connections.WaitAsync(lifetime.Token).ConfigureAwait(false);
            entered = true;
            if (!Volatile.Read(ref wanted).Contains(tile)) return;
            var path = Path.Combine(folder, tile.FileName);
            byte[]? bytes = null;
            if (File.Exists(path) && new FileInfo(path).Length is > 0 and <= 2_000_000)
            {
                try { bytes = await File.ReadAllBytesAsync(path, lifetime.Token).ConfigureAwait(false); }
                catch (IOException) { }
            }
            BitmapSource? image = null;
            if (bytes != null)
            {
                try { image = Decode(bytes); }
                catch (Exception ex) when (ex is IOException or NotSupportedException or ArgumentException or FormatException
                    or System.Runtime.InteropServices.COMException)
                { bytes = null; }
            }
            if (image == null)
            {
                if (!allowNetwork) throw new IOException("No hay una pieza HD en la caché de pruebas.");
                using var response = await Http.GetAsync(tile.Url, HttpCompletionOption.ResponseHeadersRead,
                    lifetime.Token).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    throw new IOException($"El proveedor HD respondió {(int)response.StatusCode}.");
                if (response.Content.Headers.ContentLength is > 2_000_000)
                    throw new IOException("La pieza HD supera el tamaño permitido.");
                await using var stream = await response.Content.ReadAsStreamAsync(lifetime.Token).ConfigureAwait(false);
                using var buffer = new MemoryStream();
                var block = new byte[8192];
                int read;
                while ((read = await stream.ReadAsync(block, lifetime.Token).ConfigureAwait(false)) > 0)
                {
                    if (buffer.Length + read > 2_000_000) throw new IOException("Pieza HD demasiado grande.");
                    buffer.Write(block, 0, read);
                }
                bytes = buffer.ToArray();
                image = Decode(bytes);
                try
                {
                    Directory.CreateDirectory(folder);
                    var temporary = path + ".tmp";
                    await File.WriteAllBytesAsync(temporary, bytes, lifetime.Token).ConfigureAwait(false);
                    File.Move(temporary, path, true);
                    TrimDisk();
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                { diskFailed = true; }
            }
            memory[tile] = (image, Interlocked.Increment(ref clock));
            failed.TryRemove(tile, out _);
            foreach (var key in memory.OrderBy(pair => pair.Value.Used).Take(Math.Max(0, memory.Count - 96))
                .Select(pair => pair.Key)) memory.TryRemove(key, out _);
        }
        catch (OperationCanceledException)
        {
            if (!lifetime.IsCancellationRequested)
            {
                failed[tile] = DateTime.UtcNow.AddMinutes(2);
                error = "El terreno HD tardó demasiado; se mantiene el mapa integrado.";
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException
            or NotSupportedException or ArgumentException or FormatException or System.Runtime.InteropServices.COMException)
        {
            failed[tile] = DateTime.UtcNow.AddMinutes(2);
            error = ex is NotSupportedException or System.Runtime.InteropServices.COMException
                ? "HD no compatible con este Windows; se mantiene el mapa integrado."
                : "HD incompleto o sin conexión; las zonas pendientes usan el mapa integrado.";
        }
        finally
        {
            if (entered) connections.Release();
            pending.TryRemove(tile, out _);
            if (Volatile.Read(ref disposed) == 0) changed();
        }
    }

    public static BitmapSource Decode(byte[] bytes)
    {
        if (bytes.Length < 12 || !bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            || !bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            throw new IOException("La respuesta no es una imagen WebP.");
        using var stream = new MemoryStream(bytes);
        var frame = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnDemand);
        if (frame.PixelWidth != 256 || frame.PixelHeight != 256)
            throw new IOException("Las dimensiones de la pieza HD no coinciden.");
        var cached = new CachedBitmap(frame, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        cached.Freeze();
        return cached;
    }

    private void TrimDisk()
    {
        var files = new DirectoryInfo(folder).GetFiles("43-*.webp").OrderBy(f => f.LastWriteTimeUtc).ToList();
        var size = files.Sum(f => f.Length);
        foreach (var file in files)
        {
            if (size <= 128L * 1024 * 1024) break;
            size -= file.Length;
            file.Delete();
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0) return;
        lifetime.Cancel();
        memory.Clear();
    }
}
