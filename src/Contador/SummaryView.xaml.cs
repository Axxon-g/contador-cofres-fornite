using System.Windows;
using System.Windows.Controls;

namespace Contador;

public partial class SummaryView : UserControl
{
    public event RoutedEventHandler? HistoryRequested;
    public event RoutedEventHandler? ReportRequested;
    public event RoutedEventHandler? StatisticsRequested;
    public event RoutedEventHandler? ToggleLiveRequested;

    public SummaryView() => InitializeComponent();

    private void HistoryClick(object sender, RoutedEventArgs e) => HistoryRequested?.Invoke(this, e);
    private void ReportClick(object sender, RoutedEventArgs e) => ReportRequested?.Invoke(this, e);
    private void StatisticsClick(object sender, RoutedEventArgs e) => StatisticsRequested?.Invoke(this, e);
    private void ToggleLiveClick(object sender, RoutedEventArgs e) => ToggleLiveRequested?.Invoke(this, e);
}
