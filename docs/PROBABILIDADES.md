# Tesoros y probabilidades

El apartado Tesoros usa los cofres propios de la cuenta y el período seleccionados.
No suma los cofres parciales de compañeros ni las estimaciones visuales de una
partida en curso. No modifica datos, no envía el historial y no requiere una API.

La probabilidad teórica de al menos un resultado en N intentos es
`1 - (1 - p)^N`. Supone intentos independientes con una tasa constante.
No es una predicción del siguiente cofre, una prueba de premios recibidos ni una
garantía. El historial puede ser incompleto. Para premios que cambian cada semana,
selecciona únicamente el período en que ese premio estaba disponible.

## Referencia y límites

Las tasas y multiplicadores iniciales se consultaron el 16 de noviembre de 2026 en
[TreasureOdds.cs de ayman-studio/fortnite-chest-tracker](https://github.com/ayman-studio/fortnite-chest-tracker/blob/main/src/FortniteChestTracker/TreasureOdds.cs).
Son referencia comunitaria pendiente de verificación independiente con Epic;
no se presentan como tasas oficiales vigentes. El modelo matemático y la interfaz
de Contador son una implementación propia; no se incorpora su lector ni su servidor.

La simulación permite cambiar N y aplicar los multiplicadores de Horas del tesoro
a todos los cofres simulados. Esto es un escenario hipotético: no clasifica
automáticamente el horario real de las aperturas. Al desactivar la simulación se
restauran los cofres del historial y las tasas normales, incluso si el selector
de Horas del tesoro había quedado marcado.

No se integra todavía el intercambio de datos entre amigos ni una estimación por
nombre basada en partidas multiplicadas por una media. Ambas funciones son
distintas del recuento obtenido de repeticiones.
