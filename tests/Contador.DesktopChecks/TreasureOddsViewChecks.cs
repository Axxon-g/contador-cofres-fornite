using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Contador.DesktopChecks;

internal static class TreasureOddsViewChecks
{
    public static async Task Run(Application app, string? output)
    {
        ViewsPreview.LoadStyles(app);
        var view = new TreasureOddsView { Margin = new Thickness(24) };
        var window = new Window
        {
            Content = view,
            Background = (Brush)app.Resources["Ground"],
            Width = 780,
            Height = 720,
            Left = -20000,
            Top = -20000,
            ShowInTaskbar = false,
            ShowActivated = false
        };
        var count = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            count++;
        }
        var rows = (ItemsControl)view.FindName("OddsRows");
        var simulate = (CheckBox)view.FindName("Simulate");
        var input = (TextBox)view.FindName("ChestInput");
        var boost = (CheckBox)view.FindName("Boosted");
        double Pirate() => ((TreasureOddsView.OddsRow)rows.Items[0]).Percent;
        window.Show();
        try
        {
            view.SetHistory(4832, "Cuenta de prueba", "Desde el 01/11/2026", 264);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(rows.Items.Count == 7, "Se muestran todas las tasas de referencia.");
            var played = (TextBox)view.FindName("PlayedInput");
            played.Text = "300";
            Check(((TextBlock)view.FindName("EstimateResult")).Text.Contains("36 partidas"),
                "La estimación resta las partidas ya registradas.");
            var real = Pirate();
            simulate.IsChecked = true;
            input.Text = "10000";
            Check(Pirate() > real, "La simulación cambia el cálculo.");
            boost.IsChecked = true;
            var boosted = Pirate();
            boost.IsChecked = false;
            Check(boosted > Pirate(), "El escenario mejorado aumenta la probabilidad.");
            input.Text = "-5";
            Check(rows.Items.Count == 0, "Una entrada inválida no conserva cifras anteriores.");
            input.Text = "0";
            Check(Pirate() == 0, "La simulación vacía indica cero.");
            boost.IsChecked = true;
            simulate.IsChecked = false;
            Check(Math.Abs(Pirate() - real) < 1e-12, "Salir de simulación restaura el historial sin mejora ficticia.");
            view.SetHistory(10, "Otra cuenta", "Otro período");
            Check(Pirate() < real, "El cálculo se actualiza al cambiar de cuenta o fechas.");
            Check(((TextBlock)view.FindName("EstimateResult")).Text.Contains("al menos una"),
                "Cambiar a un perfil sin muestra elimina la estimación anterior.");
            view.SetHistory(4832, "Cuenta de prueba · captura de comprobación", "Desde el 01/11/2026");
            if (output != null) Directory.CreateDirectory(output);
            foreach (var width in new[] { 780, 600 })
            {
                window.Width = width;
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                view.UpdateLayout();
                if (output == null) continue;
                var bitmap = new RenderTargetBitmap((int)view.ActualWidth, (int)view.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(view);
                var png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(bitmap));
                using var file = File.Create(Path.Combine(output, $"tesoros-{width}.png"));
                png.Save(file);
            }
            Console.WriteLine($"{count} comprobaciones de probabilidades e interfaz correctas.");
        }
        finally { window.Close(); }
    }
}
