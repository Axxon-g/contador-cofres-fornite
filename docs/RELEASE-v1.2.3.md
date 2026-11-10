# Contador 1.2.3 — Overlay unificado

- El overlay de OBS usa el HUD transparente de la partida: moneda, total, estado, última partida, sesión y tesoros.
- Ambos reciben la misma vista de datos; generar un informe durante un análisis no restaura el diseño antiguo ni borra el estado activo.
- OBS actualiza cifras y estado cada dos segundos desde un archivo local, sin servidor y sin recargar toda la página.

Abre esta versión, mantén Contador ejecutándose y actualiza una vez la fuente Navegador de OBS. Usa el archivo local `%APPDATA%\ContadorModern\overlay.html`; la app genera también `overlay-data.js` junto a él. No necesitas copiar ni ejecutar scripts manualmente.
