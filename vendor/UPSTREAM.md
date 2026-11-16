# Upstream
Repository: https://github.com/LED-N/Fortnite-ChestCounter
Commit: 1f387987737cfd606611d5b8d294d5db04af9eab

The initial import retained upstream source; the local patches documented below have subsequently modified selected files. Binary presentation resources (.ico, .woff2) and documentation screenshots are omitted because the connected retrieval tool only serves UTF-8 text. Contador.Engine compiles original source excluding the original Program.cs and embeds release notes. The original report gracefully falls back to system fonts. The original upstream build.bat/csproj are reference files; build the top-level Contador project instead.

The initial import did not change classification or attribution. Later releases include parser hardening and local analysis changes; this is not a verbatim copy of current upstream.

Contador 1.2 adapts NativeOverlay.cs to show explicit status/last-match/session/treasure labels and a lightweight presence update. ReportWriter.cs accepts an optional SessionFilter so its session totals match the desktop app. These presentation changes do not modify replay analysis.

Contador 1.2.1 adds a native icon drawing callback for the coin embedded in Contador.Engine. The icon handle is destroyed after painting; the legacy chest remains a fallback if no callback is provided.

Contador 1.2.3 routes ReportWriter.WriteAll through WriteOverlay and accepts an optional OverlayWriter callback. The desktop supplies the same immutable HUD snapshot to OBS and native presentation; the legacy writer remains the fallback for upstream entry points.

Contador 1.2.4 restyles the report with the launcher's navy/crimson/gold palette and Segoe UI. ReportBrandImage supplies a self-contained coin image. Report statistics and replay analysis are unchanged.

Contador 1.2.5 adds ReplayFileStability to compare file length/write timestamps, lowers the finalized quiet period to one second, and retries files changed during decoding. Tracker exposes report/compaction options so the desktop can publish counters before generating reports and let the runtime manage memory normally. The decoding thread uses BelowNormal priority; chest attribution/classification remain unchanged.

## Filtro local de fechas (1.2.7)

Config incorpora fechas locales opcionales; MatchPeriod compara días inclusivos sin mutar los registros. Profiles.View comparte el filtro por cuenta y fecha. La lista de perfiles conserva las cuentas sin partidas en el período, con cifras cero. ReportWriter identifica el período y exporta el CSV con la misma vista. Registros sin MatchStartUtc usan AnalyzedAtUtc.

## Contador 2.0 — local analysis changes

ReplayAnalyzer now delegates container cycles and replicated SearchingPawn state to the owned ContainerInteractionLedger. Individual and teammate accounting use the same eligibility rule. Team identity is retained per player and verified session; TeamReplayReconciliation can replace an observed teammate contribution with that player's own locally parsed replay. It never adds that contribution twice or matches by display name alone.

The multi-perspective approach was compared with LED-N/Fortnite-ChestCounter at c4ca1400ff1f79dbee4ea3ecb3f8eb72c8e6bb82 and lvortexxx/ChestTokenAssistant at 7eab883267a6b2c546a910ceaf0db7b1706b4905 (MIT). The implementation is local and does not import the latter's unsigned JSON/CSV summaries or its name fallback. Existing upstream notices remain in place. See docs/INVESTIGACION-v2.0.md for all reviewed sources, license constraints and limits. This release does not wholesale replace the parser and retains local security fixes.
## Contador 2.2

Las políticas de recuperación y las referencias de probabilidades se documentan en
[ADAPTACION-2.2.md](../docs/ADAPTACION-2.2.md). Se conserva el aviso de la referencia
MIT en LICENSE-ayman-studio.txt; no se conecta con su servicio de amigos. La UI, el
comprobador de versiones y los módulos de cálculo propios están separados del lector.
