using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Contador.Engine.Live;
using Contador.Live;
using Windows.Media.Ocr;

namespace Contador.DesktopChecks;

internal static class OcrFixtureChecks
{
    public static async Task Run()
    {
        var engine = OcrEngine.TryCreateFromUserProfileLanguages()
            ?? throw new InvalidOperationException("No hay idioma OCR de Windows para verificar el lector real.");
        using var reader = new VisualCaptureSession();
        // Exercise the real preprocessing/Windows OCR path with generated pixels only.
        // Do not call Start or capture the user's screen during this test.
        typeof(VisualCaptureSession).GetField("engine", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(reader, engine);
        var frameType = typeof(VisualCaptureSession).Assembly.GetType("Contador.Live.FortniteFrameSource+Frame")!;
        var recognize = typeof(VisualCaptureSession).GetMethod("RecognizeAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var checks = 0;
        foreach (var color in new[] { Colors.White, Color.FromRgb(215, 140, 255) })
        {
            const int width = 480, height = 160;
            var drawing = new DrawingVisual();
            using (var context = drawing.RenderOpen())
            {
                context.DrawRectangle(Brushes.Black, null, new Rect(0, 0, width, height));
                var brush = new SolidColorBrush(color);
                foreach (var (text, y) in new[] { ("+200 XP", 20), ("RARE CHEST X2", 70) })
                    context.DrawText(new FormattedText(text, CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, new Typeface("Segoe UI"), 30, brush, 1), new Point(20, y));
            }
            var image = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            image.Render(drawing);
            var pixels = new byte[width * height * 4];
            image.CopyPixels(pixels, width * 4, 0);
            var frame = Activator.CreateInstance(frameType, pixels, width, height)!;
            var counter = new VisualChestCounter();
            string[] original = [];
            foreach (var threshold in new[] { false, true })
            {
                var lines = await (Task<string[]>)recognize.Invoke(reader, [frame, threshold])!;
                var amount = VisualChestCounter.ReadPopup(lines);
                if (amount is < 1 or > 2 || (!threshold && amount != 2))
                    throw new Exception($"No reconoce el aviso generado ({color}, filtro={threshold}): {string.Join(" / ", lines)}");
                if (!threshold) original = lines;
                counter.Observe(lines);
                checks++;
            }
            counter.Observe(original);
            if (counter.Count != 2) throw new Exception("No concilia el multiplicador entre las dos variantes OCR.");
            checks++;
        }
        Console.WriteLine($"{checks} comprobaciones del OCR real correctas: blanco/violeta, color/filtro. No se capturó la pantalla.");
    }
}
