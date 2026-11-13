using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Contador.Engine;

namespace Contador;

public partial class RouteSuggestionsView : UserControl
{
    private MapPoint? revealedPoint;
    public MapPlannerModel Planner => (MapPlannerModel)DataContext;
    public InteractiveMap MapControl => Map;
    public RouteSuggestionsView() : this(new MapPlannerModel()) { }
    public RouteSuggestionsView(bool persist) : this(new MapPlannerModel(persist)) { }
    private RouteSuggestionsView(MapPlannerModel model, bool expanded = false)
    {
        InitializeComponent();
        DataContext = model;
        Map.ViewChanged += (_, _) => ZoomLabel.Text = $"{Map.Viewport.Zoom * 100:0} %";
        PropertyChangedEventManager.AddHandler(model, RevealSelection, "");
        if (expanded) ExpandButton.Visibility = Visibility.Collapsed;
    }

    private void RevealSelection(object? sender, PropertyChangedEventArgs e)
    {
        if (Planner.SelectedPoint == revealedPoint) return;
        revealedPoint = Planner.SelectedPoint;
        if (revealedPoint != null)
            Dispatcher.BeginInvoke(new Action(() => SelectionHeading.BringIntoView()));
    }

    private void ZoomIn(object sender, RoutedEventArgs e) => Map.Zoom(1.3);
    private void ZoomOut(object sender, RoutedEventArgs e) => Map.Zoom(1 / 1.3);
    private void FitMap(object sender, RoutedEventArgs e) => Map.Fit();
    private void ToggleLayers(object sender, RoutedEventArgs e)
    {
        var show = LayerPanel.Visibility != Visibility.Visible;
        LayerPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        LayersButton.Content = show ? "Ocultar capas" : "Mostrar capas";
    }
    private void ExpandMap(object sender, RoutedEventArgs e)
    {
        var window = new Window
        {
            Title = "Mapa interactivo · Contador 1.2.7", Owner = Window.GetWindow(this),
            WindowState = WindowState.Maximized, MinWidth = 860, MinHeight = 640,
            Content = new RouteSuggestionsView(Planner, true) { Margin = new Thickness(20) }
        };
        window.PreviewKeyDown += (_, key) =>
        {
            if (key.Key != Key.Escape) return;
            if (Planner.EditRoute) Planner.EditRoute = false;
            else window.Close();
            key.Handled = true;
        };
        window.Show();
    }
    private void PlaceChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MapPlannerModel model && PlacePicker.SelectedItem is string name
            && model.Catalog.Places.TryGetValue(name, out var position))
            Map.FocusPoint(position.X, position.Y, 4);
    }
    private void GuideChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MapPlannerModel && e.AddedItems.Count > 0) Map.Fit();
    }
    private void PointChosen(object sender, SelectionChangedEventArgs e)
    {
        if (sender is Control control && (control.IsKeyboardFocusWithin || control.IsMouseOver)
            && e.AddedItems.OfType<MapPoint>().FirstOrDefault() is { } point)
            Map.FocusPoint(point.X, point.Y, 5);
    }
    private void StopChosen(object sender, SelectionChangedEventArgs e)
    {
        if (RouteStops.IsKeyboardFocusWithin && e.AddedItems.OfType<PlannerRouteRow>().FirstOrDefault() is { } row)
            Map.FocusPoint(row.Node.X, row.Node.Y, 5);
    }
    private void MarkChecked(object sender, RoutedEventArgs e) => Planner.Mark(MapReview.Checked);
    private void MarkEmpty(object sender, RoutedEventArgs e) => Planner.Mark(MapReview.Empty);
    private void MarkPending(object sender, RoutedEventArgs e) => Planner.Mark(MapReview.Pending);
    private void ResetReviews(object sender, RoutedEventArgs e) => Planner.ResetReviews();
    private void AddPointStop(object sender, RoutedEventArgs e) => Planner.AddPointStop();
    private void RemoveStop(object sender, RoutedEventArgs e) => Planner.RemoveStop();
    private void ClearRoute(object sender, RoutedEventArgs e) => Planner.ClearRoute();
    private void Undo(object sender, RoutedEventArgs e) => Planner.Undo();
    private void OpenSource(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        if (e.Uri.Scheme != Uri.UriSchemeHttps) return;
        try { Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true }); }
        catch (Exception)
        {
            MessageBox.Show(Window.GetWindow(this), "No se pudo abrir el navegador. Comprueba tu navegador predeterminado y la conexión a Internet.", "Abrir fuente");
        }
    }
}
