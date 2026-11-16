using System.Windows;
using System.Windows.Controls;
using Contador.Statistics;

namespace Contador;

public partial class PlayerStatisticsView : UserControl
{
    public PlayerStatisticsViewModel Model { get; }

    public PlayerStatisticsView() : this(new PlayerStatisticsViewModel()) { }
    public PlayerStatisticsView(PlayerStatisticsViewModel model)
    {
        Model = model;
        InitializeComponent();
        DataContext = Model;
    }

    private async void SearchClick(object sender, RoutedEventArgs e) => await Model.SearchAsync();
    private void CancelClick(object sender, RoutedEventArgs e) => Model.Cancel(true);
}
