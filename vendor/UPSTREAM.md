# Upstream
Repository: https://github.com/LED-N/Fortnite-ChestCounter
Commit: 1f387987737cfd606611d5b8d294d5db04af9eab

Source files are retained verbatim. Binary presentation resources (.ico, .woff2) and documentation screenshots are omitted because the connected retrieval tool only serves UTF-8 text. Contador.Engine compiles original source excluding the original Program.cs and embeds release notes. The original report gracefully falls back to system fonts. The original upstream build.bat/csproj are reference files; build the top-level Contador project instead.

No changes to chest classification, replay decoding, deduplication or attribution.

Contador 1.2 adapts NativeOverlay.cs to show explicit status/last-match/session/treasure labels and a lightweight presence update. ReportWriter.cs accepts an optional SessionFilter so its session totals match the desktop app. These presentation changes do not modify replay analysis.

Contador 1.2.1 adds a native icon drawing callback for the coin embedded in Contador.Engine. The icon handle is destroyed after painting; the legacy chest remains a fallback if no callback is provided.

Contador 1.2.3 routes ReportWriter.WriteAll through WriteOverlay and accepts an optional OverlayWriter callback. The desktop supplies the same immutable HUD snapshot to OBS and native presentation; the legacy writer remains the fallback for upstream entry points.

Contador 1.2.4 restyles the report with the launcher's navy/crimson/gold palette and Segoe UI. ReportBrandImage supplies a self-contained coin image. Report statistics and replay analysis are unchanged.

Contador 1.2.5 adds ReplayFileStability to compare file length/write timestamps, lowers the finalized quiet period to one second, and retries files changed during decoding. Tracker exposes report/compaction options so the desktop can publish counters before generating reports and let the runtime manage memory normally. The decoding thread uses BelowNormal priority; chest attribution/classification remain unchanged.

## Filtro local de fechas (1.2.7)

Config incorpora fechas locales opcionales; MatchPeriod compara días inclusivos sin mutar los registros. Profiles.View comparte el filtro por cuenta y fecha. La lista de perfiles conserva las cuentas sin partidas en el período, con cifras cero. ReportWriter identifica el período y exporta el CSV con la misma vista. Registros sin MatchStartUtc usan AnalyzedAtUtc.
