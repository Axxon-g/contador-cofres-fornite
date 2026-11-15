using System.Windows;
using ChestCounter;
using Contador.Engine;
using Microsoft.Win32;

namespace Contador;

public partial class MainWindow
{
    private async void ClearTeamReplaysClick(object sender, RoutedEventArgs e)
    {
        if (closing || vm.IsBusy) return;
        await gate.WaitAsync();
        try
        {
            if (closing) return;
            var previous = data.TeamReplaySources;
            data.TeamReplaySources = new List<MatchRecord>();
            try { store.SaveData(data); }
            catch { data.TeamReplaySources = previous; throw; }
            Refresh();
            UpdateOverlay();
            reportWritten = false;
            vm.Footer = "Fuentes del equipo retiradas. Tus archivos y tu historial personal se conservan.";
        }
        catch (Exception ex) { ShowError(ex); }
        finally { gate.Release(); vm.NotifyAll(); }
    }

    private async void ImportTeamReplaysClick(object sender, RoutedEventArgs e)
    {
        if (closing || vm.IsBusy) return;
        var picker = new OpenFileDialog
        {
            Title = "Repeticiones finalizadas de tus compañeros",
            Filter = "Repeticiones de Fortnite (*.replay)|*.replay",
            Multiselect = true
        };
        if (picker.ShowDialog(this) != true) return;
        if (picker.FileNames.Length > 20)
        {
            vm.Footer = "Selecciona como máximo 20 repeticiones por importación.";
            vm.NotifyAll();
            return;
        }

        await gate.WaitAsync();
        try
        {
            if (closing) return;
            vm.IsBusy = true;
            vm.Status = "Leyendo repeticiones del equipo";
            vm.StatusDetail = "Se verificarán partida, identidad y pertenencia al equipo. Los archivos permanecen en tu PC.";
            vm.NotifyAll();
            var paths = picker.FileNames;
            var batch = await Task.Run(() =>
            {
                var parsed = new List<MatchRecord>();
                var errors = new List<string>();
                foreach (var path in paths)
                {
                    try { parsed.Add(TeamReplayReconciliation.ReadSource(path)); }
                    catch (Exception ex) when (ex is not OutOfMemoryException)
                    {
                        ErrorLog.Write("Importación de repetición de equipo", ex);
                        errors.Add(System.IO.Path.GetFileName(path));
                    }
                }
                return (Records: parsed, Errors: errors);
            });
            var records = batch.Records;
            var accepted = records.Where(source => data.Matches.Any(match =>
                TeamReplayReconciliation.CanUse(match, source))).ToList();
            var updated = data.TeamReplaySources.ToList();
            var retained = 0;
            foreach (var source in accepted)
            {
                bool SameFile(MatchRecord existing) => TeamReplayReconciliation.SameSession(existing.SessionId, source.SessionId)
                    && string.Equals(existing.EpicId, source.EpicId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(existing.ReplayFile, source.ReplayFile, StringComparison.OrdinalIgnoreCase);
                if (updated.Any(existing => SameFile(existing) && existing.AnalyzerVersion == source.AnalyzerVersion
                    && existing.ChestsOpened > source.ChestsOpened))
                {
                    retained++;
                    continue;
                }
                updated.RemoveAll(SameFile);
                updated.Add(source);
            }
            if (updated.Count > 1000)
                throw new InvalidOperationException("Se alcanzó el límite de 1.000 fuentes del equipo. No se modificó el registro.");

            var previous = data.TeamReplaySources;
            data.TeamReplaySources = updated;
            try { store.SaveData(data); }
            catch { data.TeamReplaySources = previous; throw; }
            Refresh();
            UpdateOverlay();
            reportWritten = false;
            vm.Status = "Registro del equipo revisado";
            vm.StatusDetail = "Las fuentes de compañeros complementan el mínimo observado; no modifican tus cofres propios.";
            vm.TeamImportSummary = $"{accepted.Count} fuentes vinculadas · {records.Count - accepted.Count} sin partida/equipo coincidentes · "
                + $"{batch.Errors.Count} archivos no aptos · {retained} resúmenes anteriores conservados por mayor cobertura."
                + (batch.Errors.Count > 0 ? " Revisa: " + string.Join(", ", batch.Errors) : "");
            vm.Footer = vm.TeamImportSummary;
        }
        catch (Exception ex) { ShowError(ex); }
        finally
        {
            vm.IsBusy = false;
            gate.Release();
            vm.NotifyAll();
        }
    }

    private async void RemoveTeamSourceClick(object sender, RoutedEventArgs e)
    {
        if (closing || vm.IsBusy || (sender as FrameworkElement)?.Tag is not Dashboard.TeamSourceRow row) return;
        await gate.WaitAsync();
        try
        {
            if (closing) return;
            var previous = data.TeamReplaySources;
            data.TeamReplaySources = previous.Where(source => !(TeamReplayReconciliation.SameSession(source.SessionId, row.SessionId)
                && string.Equals(source.EpicId, row.EpicId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(source.ReplayFile, row.File, StringComparison.OrdinalIgnoreCase))).ToList();
            try { store.SaveData(data); }
            catch { data.TeamReplaySources = previous; throw; }
            Refresh();
            UpdateOverlay();
            reportWritten = false;
            vm.TeamImportSummary = "Fuente retirada del contador. El archivo .replay y tu historial personal se conservan.";
            vm.NotifyAll();
        }
        catch (Exception ex) { ShowError(ex); }
        finally { gate.Release(); }
    }
}
