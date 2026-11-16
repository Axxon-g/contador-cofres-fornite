using System.IO;
using System.Windows;
using Contador.Engine;
using Microsoft.Win32;

namespace Contador;

public partial class MainWindow
{
    private async void ArchiveClick(object sender, RoutedEventArgs e) =>
        await SaveAsync(() => config.ArchiveReplays = ArchiveCheck.IsChecked == true);

    private void OpenArchiveClick(object sender, RoutedEventArgs e)
    {
        var path = Path.Combine(store.Folder, "ReplayArchive");
        try
        {
            Directory.CreateDirectory(path);
            OpenPath(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { ShowError(ex); }
    }

    private async void RetryFailedClick(object sender, RoutedEventArgs e)
    {
        if (closing || scanning || vm.IsBusy || !await gate.WaitAsync(0)) return;
        var count = 0;
        try
        {
            count = tracker.RetryFailedFiles();
            vm.Footer = count == 0 ? "No hay archivos fallidos disponibles para reintentar."
                : $"{count} archivos preparados para reintento. El historial se conserva.";
            vm.NotifyAll();
        }
        catch (Exception ex) { ShowError(ex); }
        finally { gate.Release(); }
        if (count > 0) await ScanAsync(userRequested: true);
    }

    private async void ExportDiagnosticsClick(object sender, RoutedEventArgs e)
    {
        if (closing || scanning || vm.IsBusy) return;
        var dialog = new SaveFileDialog
        {
            FileName = "Contador-diagnostico.json",
            Filter = "Diagnóstico JSON|*.json",
            AddExtension = true,
            DefaultExt = ".json"
        };
        if (dialog.ShowDialog(this) != true || !await gate.WaitAsync(0)) return;
        vm.IsBusy = true;
        vm.NotifyAll();
        try
        {
            var json = await Task.Run(() => ReplayDiagnostics.Export(data, config, AppVersion.Full));
            await File.WriteAllTextAsync(dialog.FileName, json);
            vm.Footer = "Diagnóstico guardado: tamaños, fechas y categorías. Sin nombres, rutas, claves ni repeticiones.";
        }
        catch (Exception ex) { ShowError(ex); }
        finally
        {
            vm.IsBusy = false;
            gate.Release();
            vm.NotifyAll();
        }
    }
}
