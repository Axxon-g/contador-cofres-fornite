using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Contador.Engine;

namespace Contador;

public partial class TreasureOddsView : UserControl
{
    public sealed record OddsRow(string Name, string ChanceLabel, string RateLabel, double Percent, string OneInLabel)
    {
        public string AccessibleLabel => $"{Name}: {ChanceLabel}. {RateLabel}";
    }

    private long historyChests;
    private int recordedMatches;
    private bool ready;
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-ES");

    public TreasureOddsView()
    {
        InitializeComponent();
        ready = true;
        SetHistory(0, "Sin cuenta seleccionada", "Todo el historial");
    }

    private void TopBlocksSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var stacked = e.NewSize.Width < 720;
        Grid.SetColumnSpan(HistoryBlock, stacked ? 2 : 1);
        Grid.SetColumn(SimulationBlock, stacked ? 0 : 1);
        Grid.SetRow(SimulationBlock, stacked ? 1 : 0);
        Grid.SetColumnSpan(SimulationBlock, stacked ? 2 : 1);
        HistoryBlock.Margin = new Thickness(0, 0, stacked ? 0 : 16, 16);
    }

    public void SetHistory(long chests, string profile, string period, int matches = 0)
    {
        historyChests = Math.Max(0, chests);
        recordedMatches = matches;
        HistoryContext.Text = $"{profile} · {period}";
        HistoryCount.Text = historyChests.ToString("N0", Culture);
        Render();
        RenderEstimate();
    }

    private void EstimateChanged(object sender, RoutedEventArgs e)
    {
        if (ready) RenderEstimate();
    }

    private void RenderEstimate()
    {
        if (recordedMatches <= 0)
        {
            EstimateResult.Text = "Necesitamos al menos una partida registrada para calcular tu media.";
            return;
        }
        if (!long.TryParse(PlayedInput.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var played)
            || played < recordedMatches || played > 10_000_000)
        {
            EstimateResult.Text = $"Introduce un total entre {recordedMatches.ToString("N0", Culture)} y 10.000.000, sin separadores.";
            return;
        }
        var estimate = MissingMatchEstimate.Calculate(played, recordedMatches, historyChests);
        EstimateResult.Text = $"{estimate.MissingMatches.ToString("N0", Culture)} partidas sin registro × "
            + $"{estimate.Mean.ToString("0.0", Culture)} cofres de media ≈ "
            + $"{estimate.EstimatedChests.ToString("N0", Culture)} cofres estimados. Tu historial no cambia.";
    }

    private void InputsChanged(object sender, RoutedEventArgs e)
    {
        if (ready) Render();
    }

    private void Render()
    {
        var simulation = Simulate.IsChecked == true;
        SimulationInputs.Visibility = simulation ? Visibility.Visible : Visibility.Collapsed;
        var chests = historyChests;
        if (simulation && (!long.TryParse(ChestInput.Text, NumberStyles.None, CultureInfo.InvariantCulture, out chests)
                           || chests < 0 || chests > 1_000_000_000))
        {
            Validation.Text = "Escribe un número entero entre 0 y 1.000.000.000, sin separadores.";
            Validation.Visibility = Visibility.Visible;
            CalculationContext.Text = "Simulación pendiente de una cantidad válida";
            OddsRows.ItemsSource = Array.Empty<OddsRow>();
            return;
        }

        Validation.Text = chests == 0 ? "Sin cofres para este cálculo. Puedes probar una cantidad sin cambiar tu historial." : "";
        Validation.Visibility = chests == 0 ? Visibility.Visible : Visibility.Collapsed;
        var boosted = simulation && Boosted.IsChecked == true;
        CalculationContext.Text = $"{(simulation ? "Simulación" : "Modelo con tu historial")} · {chests.ToString("N0", Culture)} cofres · {(boosted ? "Horas del tesoro (simulación)" : "tasas normales")}";
        OddsRows.ItemsSource = TreasureProbability.ReferenceRates.Select(rate =>
        {
            var oneIn = rate.OneIn / (boosted ? rate.TreasureHoursMultiplier : 1);
            var percent = TreasureProbability.AtLeastOne(chests, oneIn) * 100;
            var label = percent is > 0 and < 0.01 ? "< 0,01 %"
                : percent > 99.99 ? "> 99,99 %"
                : percent.ToString("0.##", Culture) + " %";
            return new OddsRow(rate.Name, label,
                $"Por cofre: aproximadamente 1 entre {oneIn.ToString("N0", Culture)}", percent, oneIn.ToString("N0", Culture));
        }).ToArray();
    }

}
