using System.Collections.ObjectModel;
using ChestCounter;
using Contador.Engine;

namespace Contador;

public sealed partial class Dashboard
{
    public sealed record TeamChoice(string Key, string Label, MatchRecord Match);
    public sealed record TeamPlayerRow(string Name, string Observed, string Count, string Source, string Coverage);
    public sealed record TeamSourceRow(string SessionId, string EpicId, string Player, string File, string Count);
    private IReadOnlyList<MatchRecord> teamSources = Array.Empty<MatchRecord>();
    private IReadOnlyList<MatchRecord> teamImports = Array.Empty<MatchRecord>();
    private TeamChoice? selectedTeamMatch;
    public ObservableCollection<TeamChoice> TeamMatches { get; } = new();
    public IReadOnlyList<TeamPlayerRow> TeamPlayers { get; private set; } = Array.Empty<TeamPlayerRow>();
    public IReadOnlyList<TeamSourceRow> TeamSourceFiles { get; private set; } = Array.Empty<TeamSourceRow>();
    public string TeamSelectedTotal { get; private set; } = "—";
    public string TeamSelectedCoverage { get; private set; } = "No hay partidas para la cuenta y las fechas seleccionadas.";
    public string TeamImportSummary { get; set; } = "Importa archivos .replay finalizados de partidas que hayas jugado con tus compañeros.";

    public TeamChoice? SelectedTeamMatch
    {
        get => selectedTeamMatch;
        set
        {
            if (selectedTeamMatch == value) return;
            selectedTeamMatch = value;
            BuildTeamSelection();
            Changed();
        }
    }

    private void RefreshTeam(IReadOnlyList<MatchRecord> matches, IReadOnlyList<MatchRecord> imports,
        IReadOnlyList<MatchRecord> local)
    {
        var selectedKey = selectedTeamMatch?.Key;
        teamImports = imports.ToList();
        teamSources = imports.Concat(local).ToList();
        TeamMatches.Clear();
        foreach (var match in matches)
            TeamMatches.Add(new TeamChoice(match.SessionId,
                $"{(match.MatchStartUtc ?? match.AnalyzedAtUtc).ToLocalTime():dd/MM/yyyy HH:mm} · {Pretty.Playlist(match.Playlist)}", match));
        selectedTeamMatch = TeamMatches.FirstOrDefault(choice => choice.Key == selectedKey) ?? TeamMatches.FirstOrDefault();
        BuildTeamSelection();
        Changed(nameof(SelectedTeamMatch));
    }

    private void BuildTeamSelection()
    {
        var match = selectedTeamMatch?.Match;
        if (match == null)
        {
            TeamPlayers = Array.Empty<TeamPlayerRow>();
            TeamSourceFiles = Array.Empty<TeamSourceRow>();
            TeamSelectedTotal = "—";
            TeamSelectedCoverage = "No hay partidas para la cuenta y las fechas seleccionadas.";
        }
        else
        {
            var result = TeamReplayReconciliation.Resolve(match, teamSources);
            TeamPlayers = new[] { new TeamPlayerRow(match.PlayerName ?? "Tú", match.ChestsOpened.ToString("N0"),
                match.ChestsOpened.ToString("N0"), "Tu repetición", "Registro personal") }
                .Concat(result.Members.Select(member => new TeamPlayerRow(member.Name,
                    member.Observed.ToString("N0"), (member.OwnPerspective ? "" : "≥ ") + member.Chests.ToString("N0"),
                    member.OwnPerspective ? "Su propia repetición"
                        : member.Supplemented ? "Otra perspectiva del equipo" : "Solo tu perspectiva",
                    member.Conflict ? "Fuentes en conflicto · se conserva el mínimo"
                        : member.OwnPerspective ? "Perspectiva vinculada" : "Parcial · falta su repetición"))).ToArray();
            TeamSelectedTotal = match.TeammateChestsOpened == null && result.Members.Count == 0 ? "—"
                : "≥ " + ((long)match.ChestsOpened + result.TeammateChests).ToString("N0");
            var ownSources = result.Members.Count(member => member.OwnPerspective);
            TeamSelectedCoverage = result.Members.Count == 0
                ? "Esta partida no tiene compañeros identificados en la repetición."
                : $"{ownSources} de {result.Members.Count} compañeros con su propia perspectiva. "
                    + "Las cifras parciales pueden omitir cofres lejanos. Importa sus repeticiones para ampliar la cobertura.";
            TeamSourceFiles = teamImports.Where(source => TeamReplayReconciliation.CanUse(match, source))
                .Select(source => new TeamSourceRow(source.SessionId, source.EpicId!, source.PlayerName ?? "Compañero",
                    source.ReplayFile, source.ChestsOpened.ToString("N0"))).ToArray();
        }
        foreach (var property in new[] { nameof(TeamPlayers), nameof(TeamSourceFiles), nameof(TeamSelectedTotal), nameof(TeamSelectedCoverage) })
            Changed(property);
    }
}
