using System.Windows;
using System.Windows.Controls;

namespace Contador;

public partial class PlayerReportView : UserControl
{
    public event EventHandler? OpenHtmlRequested;

    public PlayerReportView() => InitializeComponent();

    private void OpenHtmlClick(object sender, RoutedEventArgs e) =>
        OpenHtmlRequested?.Invoke(this, EventArgs.Empty);
}
