using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Contador.Engine.Live;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace Contador.Live;

public enum LiveCountPhase { Waiting, Counting, Paused, Unavailable }

/// <summary>Session-scoped permission: no capture is possible before Start is called.</summary>
public sealed class VisualCaptureSession : IDisposable
{
    public sealed record Snapshot(int Chests, string Status, IReadOnlyList<string> Names,
        string Diagnostic = "", LiveCountPhase Phase = LiveCountPhase.Waiting);
    private readonly VisualChestCounter counter = new();
    private OcrEngine? engine;
    private CancellationTokenSource? cancellation;
    private readonly object sync = new();
    private bool inMatch;
    private bool suspended;
    private long generation;
    public event Action<Snapshot>? Updated;

    public void Start()
    {
        if (cancellation != null) return;
        engine = OcrEngine.TryCreateFromUserProfileLanguages()
            ?? throw new InvalidOperationException("Windows no tiene un idioma de reconocimiento instalado. Instálalo en Configuración → Hora e idioma.");
        cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        _ = Task.Run(() => RunAsync(token), token);
    }

    public void SetMatch(bool value)
    {
        lock (sync)
        {
            if (inMatch == value) return;
            inMatch = value;
            generation++;
            counter.Reset();
        }
    }

    public void SetPaused(bool value)
    {
        lock (sync)
        {
            if (suspended != value) generation++;
            suspended = value;
        }
    }

    private async Task RunAsync(CancellationToken token)
    {
        var tick = 0;
        var rosterClock = Stopwatch.StartNew();
        var diagnostic = "Esperando el primer aviso de experiencia. Los cofres rotos sin aviso se revisan en el replay.";
        IReadOnlyList<string> names = Array.Empty<string>();
        try
        {
            while (!token.IsCancellationRequested)
            {
                var frameClock = Stopwatch.StartNew();
                bool active;
                bool paused;
                long version;
                lock (sync) { active = inMatch && !suspended; paused = suspended; version = generation; }
                var phase = paused ? LiveCountPhase.Paused : LiveCountPhase.Waiting;
                var status = paused ? "Reanuda la detección para continuar contando."
                    : "Activado. El conteo comenzará cuando entres a una partida.";
                if (active)
                {
                    var frame = await Task.Run(() => CaptureActive(false, version, token), token);
                    if (frame == null)
                    {
                        phase = LiveCountPhase.Paused;
                        status = "Vuelve a Fortnite para continuar. Usa el modo de pantalla en ventana sin bordes.";
                    }
                    else
                    {
                        // Alternate original colors and a white/lavender text mask. Fortnite's
                        // XP labels are not necessarily white, so a white-only mask loses them.
                        var lines = await RecognizeAsync(frame, ++tick % 2 == 0);
                        var amount = VisualChestCounter.ReadPopup(lines);
                        lock (sync)
                        {
                            if (!token.IsCancellationRequested && version == generation && inMatch && !suspended)
                                counter.Observe(lines);
                        }
                        phase = LiveCountPhase.Counting;
                        status = "Contando los avisos de cofres que aparecen mientras juegas.";
                        diagnostic = amount > 0 ? $"Aviso de {amount} cofre(s) reconocido · OCR {frameClock.ElapsedMilliseconds} ms."
                            : $"Sin aviso XP/PE de cofre reconocido · OCR {frameClock.ElapsedMilliseconds} ms. Los rotos sin aviso se revisan al finalizar.";
                    }
                }
                else names = Array.Empty<string>();
                lock (sync)
                {
                    if (!token.IsCancellationRequested && version == generation)
                        Updated?.Invoke(new Snapshot(counter.Count, status, names, diagnostic, phase));
                }
                // Publish the chest result before the slower roster recognition. Refresh names
                // sparingly; they must not stall each fifth chest frame as in the initial reader.
                if (active && rosterClock.Elapsed >= TimeSpan.FromSeconds(6))
                {
                    var roster = await Task.Run(() => CaptureActive(true, version, token), token);
                    if (roster != null) names = VisualRoster.Parse(await RecognizeAsync(roster, false));
                    rosterClock.Restart();
                }
                await Task.Delay(Math.Max(20, 150 - (int)Math.Min(150, frameClock.ElapsedMilliseconds)), token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            if (!token.IsCancellationRequested)
                Updated?.Invoke(new Snapshot(counter.Count,
                    "No se pudo continuar el conteo. Desactívalo y vuelve a activarlo; tu historial sigue funcionando.",
                    Array.Empty<string>(), Phase: LiveCountPhase.Unavailable));
        }
    }

    private FortniteFrameSource.Frame? CaptureActive(bool roster, long version, CancellationToken token)
    {
        lock (sync)
        {
            if (token.IsCancellationRequested || !inMatch || suspended || generation != version) return null;
            return FortniteFrameSource.Capture(roster);
        }
    }

    private async Task<string[]> RecognizeAsync(FortniteFrameSource.Frame frame, bool threshold)
    {
        // Enlarge small HUD labels before OCR, bounded by the Windows engine limit.
        // Pixels and OCR output remain in memory only.
        var scale = frame.Width < 900 ? 2 : 1;
        if (frame.Width * scale > OcrEngine.MaxImageDimension || frame.Height * scale > OcrEngine.MaxImageDimension)
            scale = 1;
        var width = frame.Width * scale;
        var height = frame.Height * scale;
        var pixels = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var source = ((y / scale) * frame.Width + x / scale) * 4;
                var target = (y * width + x) * 4;
                var blue = frame.Pixels[source];
                var green = frame.Pixels[source + 1];
                var red = frame.Pixels[source + 2];
                var white = blue >= 175 && green >= 175 && red >= 175;
                var lavender = blue >= 175 && red >= 150 && green >= 85 && blue >= green + 20 && red >= green + 10;
                var ink = white || lavender ? (byte)0 : (byte)255;
                pixels[target] = threshold ? ink : blue;
                pixels[target + 1] = threshold ? ink : green;
                pixels[target + 2] = threshold ? ink : red;
                pixels[target + 3] = 255;
            }
        }
        using var bitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Ignore);
        bitmap.CopyFromBuffer(pixels.AsBuffer());
        var result = await engine!.RecognizeAsync(bitmap);
        return result.Lines.Take(40).Select(line => line.Text).ToArray();
    }

    public void Dispose()
    {
        cancellation?.Cancel();
        // A fresh session instance is required to grant permission again.
        lock (sync) { inMatch = false; generation++; counter.Reset(); }
    }
}
