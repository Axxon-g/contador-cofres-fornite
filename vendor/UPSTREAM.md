# Upstream
Repository: https://github.com/LED-N/Fortnite-ChestCounter
Commit: 1f387987737cfd606611d5b8d294d5db04af9eab

Source files are retained verbatim. Binary presentation resources (.ico, .woff2) and documentation screenshots are omitted because the connected retrieval tool only serves UTF-8 text. Contador.Engine compiles original source excluding the original Program.cs and embeds release notes. The original report gracefully falls back to system fonts. The original upstream build.bat/csproj are reference files; build the top-level Contador project instead.

No changes to chest classification, replay decoding, deduplication or attribution.

Contador 1.2 adapts NativeOverlay.cs to show explicit status/last-match/session/treasure labels and a lightweight presence update. ReportWriter.cs accepts an optional SessionFilter so its session totals match the desktop app. These presentation changes do not modify replay analysis.

Contador 1.2.1 adds a native icon drawing callback for the coin embedded in Contador.Engine. The icon handle is destroyed after painting; the legacy chest remains a fallback if no callback is provided.
