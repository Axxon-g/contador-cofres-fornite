using System.Windows;
using Contador.Live;
using Contador.Engine;
using ChestCounter;

namespace Contador;

public partial class MainWindow
{
    private VisualCaptureSession? visualSession;
    private IReadOnlyList<string> visualNames = Array.Empty<string>();
    private int visualEstimate;
    private string visualMessage = "";
    private bool visualMatch;
    private string? visualReplay;

    private void SetVisualRecording(string? activeReplay)
    {
        if (string.Equals(visualReplay, activeReplay, StringComparison.OrdinalIgnoreCase)) return;
        visualReplay = activeReplay;
        visualSession?.SetMatch(false);
        visualEstimate = 0;
        visualNames = Array.Empty<string>();
    }

    private void VisualCaptureClick(object sender, RoutedEventArgs e)
    {
        if (visualSession != null)
        {
            StopVisualCapture();
            return;
        }
        var answer = MessageBox.Show(this,
            "¿Quieres activar el conteo en tiempo real durante esta ejecución?\n\n"
            + "Se capturan únicamente el área de avisos de experiencia y los nombres del equipo, "
            + "solo con Fortnite en primer plano y una partida detectada. Windows reconoce el texto en tu PC. "
            + "No se guardan imágenes, no se envían capturas y no se accede a la memoria del juego.\n\n"
            + "Si otro aviso se superpone en esas zonas, puede aparecer en el recorte temporal.\n\n"
            + "Es experimental: puede omitir o confundir avisos, y no conoce los cofres de compañeros lejanos. "
            + "Las cifras visuales no se suman al historial. Puedes detenerlo en cualquier momento. "
            + "Al volver a abrir Contador estará desactivado.\n\n"
            + "Si eliges No, el contador seguirá funcionando mediante repeticiones.",
            "Activar conteo en tiempo real", MessageBoxButton.YesNo, MessageBoxImage.Question,
            MessageBoxResult.No);
        if (answer != MessageBoxResult.Yes)
        {
            vm.VisualStatus = "Conteo en tiempo real desactivado. Tu historial seguirá actualizándose al terminar cada partida.";
            vm.NotifyAll();
            return;
        }
        try
        {
            var session = new VisualCaptureSession();
            session.Updated += snapshot => Dispatcher.BeginInvoke(new Action(() =>
            {
                if (closing || visualSession != session) return;
                visualEstimate = snapshot.Chests;
                visualMessage = snapshot.Status;
                vm.VisualPhase = snapshot.Phase;
                visualNames = snapshot.Names;
                if (vm.VisualDiagnostic != snapshot.Diagnostic && snapshot.Diagnostic.Length > 0)
                {
                    vm.VisualDiagnostic = snapshot.Diagnostic;
                    vm.Changed(nameof(Dashboard.VisualDiagnostic));
                }
                RefreshLiveMatch();
                if (!scanning && !vm.IsBusy) UpdateOverlay();
            }));
            visualSession = session;
            session.SetMatch(recording);
            session.SetPaused(vm.Paused);
            session.Start();
            vm.VisualCaptureEnabled = true;
            vm.VisualPhase = recording ? LiveCountPhase.Paused : LiveCountPhase.Waiting;
            vm.VisualDiagnostic = "Esperando el primer aviso de cofre.";
            vm.Changed(nameof(Dashboard.VisualDiagnostic));
            visualMessage = recording ? "Activado. Vuelve a Fortnite para empezar a contar."
                : "Activado. El conteo comenzará cuando entres a una partida.";
            RefreshLiveMatch();
        }
        catch (Exception)
        {
            StopVisualCapture();
            vm.VisualStatus = "No se pudo activar el conteo. Comprueba los idiomas de reconocimiento en Ajustes de Windows. Tu historial sigue funcionando.";
            vm.NotifyAll();
        }
    }

    private void StopVisualCapture()
    {
        visualSession?.Dispose();
        visualSession = null;
        visualNames = Array.Empty<string>();
        visualEstimate = 0;
        vm.VisualCaptureEnabled = false;
        vm.VisualStatus = "Actívalo para ver una estimación de tus cofres mientras juegas.";
        vm.VisualDiagnostic = "El conteo está desactivado.";
        RefreshLiveMatch();
        vm.Changed(nameof(Dashboard.VisualStatus));
        if (!closing && !scanning && !vm.IsBusy) UpdateOverlay();
    }

    private void RefreshLiveMatch()
    {
        var previous = (vm.VisualCount, vm.VisualStatus, vm.SquadContext);
        var previousRows = vm.SquadRows;
        if (visualMatch != recording)
        {
            visualMatch = recording;
            visualNames = Array.Empty<string>();
            visualEstimate = 0;
        }
        visualSession?.SetMatch(recording);
        visualSession?.SetPaused(vm.Paused);
        vm.VisualCount = visualSession != null && recording ? "≈ " + visualEstimate : "—";
        if (visualSession != null) vm.VisualStatus = visualMessage;
        if (recording)
        {
            vm.SquadContext = "Tu equipo en esta partida · nombres por confirmar";
            vm.SquadRows = visualSession == null ? Array.Empty<Dashboard.SquadRow>()
                : visualNames.Select(name => new Dashboard.SquadRow(name, "—", "Cofres disponibles al finalizar")).ToArray();
        }
        else if (!scanning && !vm.IsBusy)
        {
            var last = Profiles.View(data, config).Matches
                .OrderByDescending(match => match.MatchStartUtc ?? match.AnalyzedAtUtc).FirstOrDefault();
            vm.SquadContext = last == null ? "Juega una partida para ver el equipo."
                : "Equipo de la última partida · cofres registrados por jugador";
            vm.SquadRows = last == null ? Array.Empty<Dashboard.SquadRow>()
                : new[] { new Dashboard.SquadRow(last.PlayerName ?? "Tú", last.ChestsOpened.ToString("N0"), "Repetición propia") }
                    .Concat(TeamReplayReconciliation.Resolve(last, data.TeamReplaySources.Concat(data.Matches)).Members
                        .Select(member => new Dashboard.SquadRow(member.Name,
                            (member.OwnPerspective ? "" : "≥ ") + member.Chests.ToString("N0"),
                            member.OwnPerspective ? "Repetición del compañero · vinculada"
                                : member.Supplemented ? "Otra perspectiva · registro parcial" : "Solo tu repetición · registro parcial")))
                    .ToArray();
        }
        if (previous.VisualCount != vm.VisualCount) vm.Changed(nameof(Dashboard.VisualCount));
        if (previous.VisualStatus != vm.VisualStatus) vm.Changed(nameof(Dashboard.VisualStatus));
        if (previous.SquadContext != vm.SquadContext) vm.Changed(nameof(Dashboard.SquadContext));
        if (!previousRows.SequenceEqual(vm.SquadRows))
        {
            vm.Changed(nameof(Dashboard.SquadRows));
            vm.Changed(nameof(Dashboard.HasSquadRows));
        }
    }
}
