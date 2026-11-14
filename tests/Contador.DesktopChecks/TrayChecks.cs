using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using Contador;

namespace Contador.DesktopChecks;

internal static class TrayChecks
{
    public static async Task Run(Application app)
    {
        ViewsPreview.LoadStyles(app);
        var window = new Window
        {
            Width = 160, Height = 120, Left = -20000, Top = -20000,
            ShowActivated = false, ShowInTaskbar = false
        };
        var checks = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            checks++;
        }
        var choice = WindowCloseChoice.Cancel;
        var saves = 0;
        var stops = 0;
        var prompts = 0;
        var inClosing = false;
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var controller = new WindowCloseController(window, () => { saves++; return Task.CompletedTask; },
            () => stops++, error => throw error, () =>
            {
                Check(!inClosing, "La elección debe abrirse después del evento Closing.");
                prompts++;
                return choice;
            });
        window.Closing += (_, _) =>
        {
            inClosing = true;
            window.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => inClosing = false));
        };
        window.Closed += (_, _) => closed.TrySetResult();
        var ticks = 0;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
        timer.Tick += (_, _) => ticks++;
        window.Show();
        timer.Start();
        try
        {
            window.Close();
            await Task.Delay(50);
            Check(window.IsVisible && saves == 0 && stops == 0, "Cancelar mantiene la ventana y la detección.");
            choice = WindowCloseChoice.Background;
            window.Close();
            await Task.Delay(50);
            Check(!window.IsVisible && !closed.Task.IsCompleted && saves == 0 && stops == 0,
                "Segundo plano oculta sin detener ni cerrar.");
            var before = ticks;
            await Task.Delay(60);
            Check(ticks > before, "El dispatcher continúa trabajando mientras la ventana está oculta.");
            window.Show();
            Check(window.IsVisible, "La ventana oculta se puede restaurar.");
            var noTray = new CloseChoiceWindow(window, false);
            var background = ((StackPanel)noTray.Content).Children.OfType<Button>()
                .Single(button => Equals(button.Content, "Mantener en segundo plano"));
            Check(!background.IsEnabled, "Sin icono disponible no se ofrece ocultar la aplicación.");
            noTray.Close();
            choice = WindowCloseChoice.Cancel;
            controller.RequestExit();
            await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Check(saves == 1 && stops == 1 && prompts == 2,
                "Salir desde la bandeja omite la pregunta y guarda una sola vez.");
            Console.WriteLine($"{checks} comprobaciones de segundo plano correctas.");
        }
        finally { timer.Stop(); }
    }

    public static void Native()
    {
        using var tray = new TrayIcon(() => { }, () => { });
        if (!tray.IsAvailable) throw new InvalidOperationException($"El entorno no registró el icono en la bandeja de Windows (Win32 {tray.RegistrationError}).");
        tray.SetBackground(true);
        tray.Dispose();
        if (tray.IsAvailable) throw new InvalidOperationException("El icono no se retiró al salir.");
        Console.WriteLine("2 comprobaciones de bandeja nativa correctas: registro y retirada.");
    }

    public static void Render(Application app, string path)
    {
        ViewsPreview.LoadStyles(app);
        var owner = new Window { Left = -20000, Top = -20000, ShowActivated = false, ShowInTaskbar = false };
        owner.Show();
        var dialog = new CloseChoiceWindow(owner, true)
        {
            Left = -20000, Top = -20000, WindowStartupLocation = WindowStartupLocation.Manual,
            ShowActivated = false
        };
        dialog.Show();
        dialog.UpdateLayout();
        var content = (FrameworkElement)dialog.Content;
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(content.ActualWidth + 48),
            (int)Math.Ceiling(content.ActualHeight + 48), 96, 96, PixelFormats.Pbgra32);
        var drawing = new DrawingVisual();
        using (var dc = drawing.RenderOpen())
        {
            dc.DrawRectangle((Brush)app.Resources["Ground"], null, new Rect(0, 0, bitmap.Width, bitmap.Height));
            dc.DrawRectangle(new VisualBrush(content), null,
                new Rect(24, 24, content.ActualWidth, content.ActualHeight));
        }
        bitmap.Render(drawing);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using (var output = File.Create(path)) encoder.Save(output);
        dialog.Close();
        owner.Close();
    }
}
