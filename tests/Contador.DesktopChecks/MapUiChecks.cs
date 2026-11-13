using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Contador;
using Contador.Engine;

namespace Contador.DesktopChecks;

internal static class MapUiChecks
{
    public static async Task Run(Application app)
    {
        ViewsPreview.LoadStyles(app);
        var view = new RouteSuggestionsView(false);
        var window = new Window
        {
            Content = view, Width = 1000, Height = 800, Left = -20000, Top = -20000,
            ShowActivated = false, ShowInTaskbar = false
        };
        var checks = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            checks++;
        }
        void Click(string caption) => Descendants<Button>(view).First(button => Equals(button.Content, caption))
            .RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        window.Show();
        try
        {
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(view.Planner.FilteredPoints.Count == 1050, "Capas iniciales: cofres, raros, enterrados y lugares");
            Click("+");
            Check(view.MapControl.Viewport.Zoom > 1, "Botón acercar conectado al mapa");
            Click("Ver isla");
            Check(view.MapControl.Viewport.Zoom == 1, "Botón ver isla restaura zoom");
            var width = view.MapControl.ActualWidth;
            ((Button)view.FindName("LayersButton")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            window.UpdateLayout();
            Check(view.MapControl.ActualWidth > width, "Ocultar capas amplía el área del mapa");
            ((Button)view.FindName("LayersButton")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            ((ComboBox)view.FindName("PlacePicker")).SelectedItem = "Arlong Park";
            Check(view.MapControl.Viewport.Zoom == 4, "Selector centra el lugar");
            ((ComboBox)view.FindName("RoutePicker")).SelectedIndex = 0;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(view.Planner.Plan.State.Route.Count == 3, "Selector carga ruta sugerida en el editor");
            var layer = Descendants<CheckBox>(view).First(box => box.DataContext is MapLayer { Key: "chest" });
            layer.IsChecked = false;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(view.Planner.FilteredPoints.All(p => p.Kind != "chest"), "Casilla de capa filtra puntos");
            ((TextBox)view.FindName("PointSearch")).Text = "Lantern Landing";
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(view.Planner.FilteredPoints.Count > 0 && view.Planner.FilteredPoints.All(p => p.ListLabel.Contains("Lantern Landing")),
                "Búsqueda actualiza resultados");
            view.Planner.Search = "";
            var point = view.Planner.FilteredPoints.First(p => p.Kind == "buried_chest");
            view.Planner.SelectGroup([point]);
            Click("Añadir a mi ruta");
            Check(view.Planner.Plan.State.Route.Count == 4, "Detalle añade el punto seleccionado a la ruta");
            Click("Revisado");
            Check(view.Planner.Plan.State.Reviews.Count == 1, "Revisión manual guarda solo el punto elegido");
            view.Planner.HideReviewed = true;
            Check(!view.Planner.FilteredPoints.Contains(point), "Ocultar revisados elimina el punto de resultados");
            Click("Deshacer");
            Check(view.Planner.Plan.State.Reviews.Count == 0, "Deshacer restaura la revisión");
            view.Planner.Plan.State.Layers = ["chest"];
            view.Planner.SelectedStop = view.Planner.RouteRows[0];
            Click("Quitar parada");
            Check(view.Planner.Plan.State.Route.Count == 3, "Quitar parada conectado al editor");
            Click("Vaciar ruta");
            Check(view.Planner.Plan.State.Route.Count == 0, "Vaciar ruta no altera catálogo");
            Click("Deshacer");
            Check(view.Planner.Plan.State.Route.Count == 3, "Deshacer recupera ruta vaciada");
            Console.WriteLine($"{checks} comprobaciones WPF del mapa interactivo correctas.");
        }
        finally { window.Close(); }
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match) yield return match;
            foreach (var nested in Descendants<T>(child)) yield return nested;
        }
    }
}
