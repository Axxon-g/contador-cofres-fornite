using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using Contador.Engine.Updates;

namespace Contador;

public partial class MainWindow
{
    private static readonly HttpClient ReleaseHttp = new(new HttpClientHandler
    {
        AllowAutoRedirect = false,
        UseCookies = false,
        UseDefaultCredentials = false
    });
    private readonly CancellationTokenSource releaseCancellation = new();
    private bool checkingRelease;
    private bool releaseAvailable;

    private async Task CheckReleaseAsync()
    {
        if (checkingRelease || closing) return;
        checkingRelease = true;
        CheckUpdateButton.IsEnabled = SidebarUpdateButton.IsEnabled = false;
        SidebarUpdateLabel.Text = "Buscando…";
        SidebarUpdateStatus.Text = "Comprobando GitHub…";
        UpdateStatus.Text = "Buscando una nueva versión…";
        try
        {
            var checker = new ReleaseChecker(ReleaseHttp);
            var result = await checker.CheckAsync(Version.Parse(AppVersion.Full), releaseCancellation.Token);
            if (closing) return;
            UpdateStatus.Text = result.State switch
            {
                ReleaseCheckState.Available => $"Hay una nueva versión: {result.Version}.",
                ReleaseCheckState.Current => $"Ya tienes la última versión. Instalada: {AppVersion.Display}.",
                _ => "No se pudo comprobar. Revisa tu conexión e inténtalo más tarde."
            };
            if (result.State != ReleaseCheckState.Unavailable)
            {
                releaseAvailable = result.State == ReleaseCheckState.Available;
                UpdateNotice.Visibility = releaseAvailable ? Visibility.Visible : Visibility.Collapsed;
                UpdateNoticeText.Text = releaseAvailable ? $"Contador {result.Version} está disponible" : "";
            }
            SidebarUpdateStatus.Text = result.State switch
            {
                ReleaseCheckState.Available => $"Versión {result.Version} disponible.",
                ReleaseCheckState.Current => "Ya tienes la última versión.",
                _ => "No se pudo comprobar. Inténtalo de nuevo."
            };
            SidebarUpdateButton.ToolTip = releaseAvailable
                ? "Abrir la nueva versión en las descargas oficiales de GitHub"
                : "Comprobar si hay una versión nueva en GitHub";
        }
        catch (OperationCanceledException) when (releaseCancellation.IsCancellationRequested) { }
        finally
        {
            checkingRelease = false;
            if (!closing)
            {
                CheckUpdateButton.IsEnabled = SidebarUpdateButton.IsEnabled = true;
                SidebarUpdateLabel.Text = releaseAvailable ? "Nueva actualización" : "Buscar actualizaciones";
            }
        }
    }

    private async void SidebarUpdateClick(object sender, RoutedEventArgs e)
    {
        if (releaseAvailable) OpenUpdateClick(sender, e);
        else await CheckReleaseAsync();
    }

    private async void CheckUpdateClick(object sender, RoutedEventArgs e) => await CheckReleaseAsync();

    private void OpenUpdateClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(ReleaseChecker.ReleasesPage) { UseShellExecute = true });
        }
        catch (Exception error) when (error is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            UpdateStatus.Text = "No se pudo abrir el navegador. Puedes descargar la versión en GitHub desde tu navegador.";
        }
    }

    private void DismissUpdateClick(object sender, RoutedEventArgs e) => UpdateNotice.Visibility = Visibility.Collapsed;
}
