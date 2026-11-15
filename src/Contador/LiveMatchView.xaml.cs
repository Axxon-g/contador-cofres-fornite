using System.Windows.Controls;
using System.Windows;

namespace Contador;

public partial class LiveMatchView : UserControl
{
    public LiveMatchView() => InitializeComponent();
    public event RoutedEventHandler? ToggleRequested;
    private void ToggleClick(object sender, RoutedEventArgs e) => ToggleRequested?.Invoke(this, e);
}
