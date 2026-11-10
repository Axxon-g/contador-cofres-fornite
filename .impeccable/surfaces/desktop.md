# Desktop — Operate

Native Windows desktop, C# WPF (`net10.0-windows`). Source authority: `src/Contador/App.xaml`, `MainWindow.xaml`, and navigation/status assignments in `MainWindow.xaml.cs`. Root `DESIGN.md` records the extracted system; `PRODUCT.md` retains product truth.

The navy workspace, pirate-red navigation rail, ivory text and treasure-gold count/active navigation establish the night-session atmosphere. The user-supplied pirate token is embedded as a WPF Resource, displayed at 112 × 112 with Uniform stretch and accessible name “Moneda de tesoro pirata”; it needs no network image request. Segoe UI and Windows controls keep the application familiar. Ready state uses pale green; every status also has text. This is a code-led surface with no comp.

## Composition

Main window 1120 × 780; minimum 860 × 640, in WPF device-independent units. Fixed 200-unit navigation rail; flexible work area with 32 horizontal / 28 vertical inset. Persistent page title, status panel and footer frame one of three workspaces: Resumen, Historial, Ajustes. Summary and settings scroll vertically. History keeps a sortable read-only table and search/export actions.

The total is the strongest figure (76, SemiBold, gold); session count is secondary (42, SemiBold). Page titles use 28; section titles 20. Status and help panels use 10-unit corners; buttons use 8. Buttons have 18 × 11 padding, gold hover border and a 2-unit gold keyboard-focus border; disabled buttons use 0.45 opacity. Tables use 48-unit rows, alternating ground tones and visible selected-row fill. Depth is tonal rather than shadow-based. No ornamental motion is defined.

## Implementation boundary

Use native controls and OS folder/export dialogs. Preserve keyboard access, named inputs and state text. The inherited overlay is independently configurable. The replay reader remains the source of supported-format counting; do not show fabricated match data or imply live-game counting. Presence separately probes headers of recently written replay files (three-minute window), without reading the game process. The gold banner reads “EN PARTIDA · Cofres al finalizar” or “Esperando partida”; completed replay processing supplies the figures.

Session means completed matches since opening the app, excluding baseline history and historical imports. Dashboard, native overlay and report session filter share this rule, scoped to the selected account. Dashboard copy reads “Cofres de esta sesión” and “N partidas desde que abriste la app”. Overlay labels distinguish total, “Esta sesión”, “Última partida”, “Tesoros” and presence (“En partida” / “Esperando”); presence does not imply a live chest count.

`.impeccable/design.json` records extracted color metadata in schema version 2. Root DESIGN.md owns extracted token primitives in frontmatter; sidecar color metadata references the same names. No tonal ramps, browser component previews, web hook or detector were fabricated: this is WPF, with no web component library. Empty motion/shadow/breakpoint arrays mean none were extracted; window constraints are documented here rather than misrepresented as CSS breakpoints.

## Evidence boundary

v1.2 source and screenshot review is accepted for shipping. Publish succeeded: `dist/1.2.0/Contador.exe` reports version 1.2.0.0. All 35 runtime checks passed with copies of actual histories, and native overlay snapshots passed. Current review captures are in the task workspace `work/v12-review`; earlier captures remain in `.impeccable/review`. No browser detector applies to this native Windows surface.
