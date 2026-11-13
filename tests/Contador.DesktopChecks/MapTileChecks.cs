using System.IO;
using Contador;
using Contador.Engine;

namespace Contador.DesktopChecks;

internal static class MapTileChecks
{
    public static async Task Run(string fixture)
    {
        var bytes = await File.ReadAllBytesAsync(fixture);
        var image = MapTileCache.Decode(bytes);
        if (image.PixelWidth != 256 || !image.IsFrozen)
            throw new InvalidOperationException("La pieza HD real no se decodificó como imagen inmutable.");
        try
        {
            MapTileCache.Decode("<html>error del proveedor</html>"u8.ToArray());
            throw new InvalidOperationException("Una respuesta HTML se aceptó como imagen.");
        }
        catch (IOException) { }
        var folder = Path.Combine(Path.GetTempPath(), "ContadorTiles-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        var tile = new MapTile(6, 29, 34);
        await File.WriteAllBytesAsync(Path.Combine(folder, tile.FileName), bytes);
        var update = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cache = new MapTileCache(() => update.TrySetResult(), folder, allowNetwork: false);
        cache.Request([tile]);
        await update.Task.WaitAsync(TimeSpan.FromSeconds(8));
        if (cache.Get(tile) is not { IsFrozen: true })
            throw new InvalidOperationException("La pieza almacenada no se recuperó sin red.");
        var missing = new MapTile(6, 30, 34);
        var fallback = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var offline = new MapTileCache(() => fallback.TrySetResult(), folder, allowNetwork: false);
        offline.Request([missing]);
        await fallback.Task.WaitAsync(TimeSpan.FromSeconds(8));
        if (offline.Get(missing) != null || !offline.Status.Contains("incompleto"))
            throw new InvalidOperationException("Una pieza ausente debe conservar el fondo y mostrar un aviso.");
        Console.WriteLine("4 comprobaciones HD correctas: WebP real, rechazo de HTML, caché sin red y pieza ausente.");
    }
}
