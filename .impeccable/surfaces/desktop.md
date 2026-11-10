# Desktop — Operate

Native Windows desktop, C# WPF (`net10.0-windows`). Source authority: `src/Contador/App.xaml`, `MainWindow.xaml`, and navigation/status assignments in `MainWindow.xaml.cs`. Root `DESIGN.md` records the extracted system; `PRODUCT.md` retains product truth.

The charcoal workspace, pine navigation rail and gold count/active navigation establish the night-session atmosphere. Segoe UI and Windows controls keep the application familiar. Ready state uses pale green; every status also has text. This is a code-led surface with no comp.

## Composition

Main window 1120 × 780; minimum 860 × 640, in WPF device-independent units. Fixed 200-unit navigation rail; flexible work area with 32 horizontal / 28 vertical inset. Persistent page title, status panel and footer frame one of three workspaces: Resumen, Historial, Ajustes. Summary and settings scroll vertically. History keeps a sortable read-only table and search/export actions.

The total is the strongest figure (76, SemiBold, gold); session count is secondary (42, SemiBold). Page titles use 28; section titles 20. Status and help panels use 10-unit corners; buttons use 8. Buttons have 18 × 11 padding, gold hover border and a 2-unit gold keyboard-focus border; disabled buttons use 0.45 opacity. Tables use 48-unit rows, alternating ground tones and visible selected-row fill. Depth is tonal rather than shadow-based. No ornamental motion is defined.

## Implementation boundary

Use native controls and OS folder/export dialogs. Preserve keyboard access, named inputs and state text. The inherited overlay is independently configurable. The replay reader remains the source of supported-format counting; do not show fabricated match data or imply live-game counting.

`.impeccable/design.json` records extracted color metadata in schema version 2. Root DESIGN.md owns extracted token primitives in frontmatter; sidecar color metadata references the same names. No tonal ramps, browser component previews, web hook or detector were fabricated: this is WPF, with no web component library. Empty motion/shadow/breakpoint arrays mean none were extracted; window constraints are documented here rather than misrepresented as CSS breakpoints.

## Evidence boundary

Final review accepted source/preview-scoped delivery. Build, native runtime behavior, live resizing, keyboard interaction and replay count accuracy remain unverified. Existing review captures are in `.impeccable/review`; they do not establish a successful native run.

