using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Contador;
using Contador.Live;

namespace Contador.DesktopChecks;

internal static class LiveViewChecks
{
    public static async Task Run(Application app, string? output)
    {
        ViewsPreview.LoadStyles(app);
        var vm = new Dashboard();
        var checks = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            checks++;
        }
        var view = new LiveMatchView { DataContext = vm, Margin = new Thickness(24) };
        var host = new Border { Background = (Brush)app.Resources["Panel"], Child = view };
        var window = new Window
        {
            Content = host,
            Width = 780,
            Height = 620,
            Left = -20000,
            Top = -20000,
            ShowInTaskbar = false,
            ShowActivated = false
        };
        var action = (Button)view.FindName("CountAction");
        var badge = (TextBlock)view.FindName("CountState");
        var details = (Expander)view.FindName("CountDetails");
        var squad = (StackPanel)view.FindName("SquadSection");
        var metric = (StackPanel)view.FindName("LiveMetric");
        var requests = 0;
        view.ToggleRequested += (_, _) => requests++;
        window.Show();
        try
        {
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(!vm.VisualCaptureEnabled && vm.VisualCount == "—", "El conteo debe iniciar apagado y sin cifras inventadas.");
            Check(badge.Text == "Desactivado" && vm.VisualCaptureAction == "Activar conteo en tiempo real",
                "El estado inicial y la acción deben explicar el conteo en tiempo real.");
            Check(details.Visibility == Visibility.Collapsed && squad.Visibility == Visibility.Collapsed,
                "No se muestran diagnósticos ni filas vacías con el conteo apagado.");
            Check(metric.Visibility == Visibility.Collapsed, "El conteo apagado no muestra una cifra vacía que parezca un fallo.");
            Check(((SolidColorBrush)action.Background).Color == ((SolidColorBrush)app.Resources["Gold"]).Color,
                "La activación debe destacarse como acción principal.");
            action.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Check(requests == 1 && !vm.VisualCaptureEnabled, "El botón solicita la acción sin conceder permiso por sí mismo.");
            await Save("desactivado");

            vm.VisualCaptureEnabled = true;
            vm.VisualPhase = LiveCountPhase.Waiting;
            vm.VisualStatus = "Activado. El conteo comenzará cuando entres a una partida.";
            vm.NotifyAll();
            await Dispatcher.Yield(DispatcherPriority.DataBind);
            Check(badge.Text == "Esperando partida", "Activado sin partida no debe decir que ya está contando.");
            Check(details.Visibility == Visibility.Visible && !details.IsExpanded,
                "Los detalles técnicos deben estar disponibles pero cerrados por defecto.");
            await Save("esperando");

            vm.VisualPhase = LiveCountPhase.Counting;
            vm.VisualCount = "≈ 8";
            vm.VisualStatus = "Contando los avisos de cofres que aparecen mientras juegas.";
            vm.SquadContext = "Equipo de prueba · datos ficticios";
            vm.SquadRows = [new("Compañero de prueba con nombre largo", "—", "Cofres disponibles al finalizar")];
            vm.NotifyAll();
            await Dispatcher.Yield(DispatcherPriority.DataBind);
            Check(badge.Text == "Contando" && vm.IsVisualCounting, "El estado activo debe distinguirse del estado de espera.");
            Check(vm.Total == "0", "La estimación no modifica el historial confirmado.");
            Check(squad.Visibility == Visibility.Visible && vm.VisualCaptureAction == "Desactivar conteo",
                "Solo se muestran jugadores cuando hay datos y se ofrece detener el conteo.");
            await Save("contando");

            vm.VisualPhase = LiveCountPhase.Paused;
            vm.VisualStatus = "Vuelve a Fortnite para continuar. Usa el modo de pantalla en ventana sin bordes.";
            await Dispatcher.Yield(DispatcherPriority.DataBind);
            vm.NotifyAll();
            await Dispatcher.Yield(DispatcherPriority.DataBind);
            Check(badge.Text == "En pausa" && !vm.IsVisualCounting && vm.VisualCount == "≈ 8",
                "La pausa conserva el valor y no se presenta como un conteo activo.");
            await Save("pausa");

            vm.VisualCaptureEnabled = false;
            vm.VisualCount = "—";
            vm.SquadRows = [];
            vm.NotifyAll();
            await Dispatcher.Yield(DispatcherPriority.DataBind);
            Check(details.Visibility == Visibility.Collapsed && squad.Visibility == Visibility.Collapsed,
                "Desactivar debe volver a ocultar detalles y jugadores vacíos.");
            Console.WriteLine($"{checks} comprobaciones de experiencia del conteo correctas. Sin capturas reales.");
        }
        finally { window.Close(); }

        async Task Save(string state)
        {
            if (output == null) return;
            Directory.CreateDirectory(output);
            foreach (var width in new[] { 780, 520 })
            {
                window.Width = width;
                window.UpdateLayout();
                await Dispatcher.Yield(DispatcherPriority.Render);
                var bitmap = new RenderTargetBitmap((int)host.ActualWidth, (int)host.ActualHeight,
                    96, 96, PixelFormats.Pbgra32);
                bitmap.Render(host);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var file = File.Create(Path.Combine(output, $"conteo-{state}-{width}.png"));
                encoder.Save(file);
            }
        }
    }
}
