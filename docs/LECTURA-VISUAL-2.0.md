# Conteo en tiempo real · experimental en 2.0

## Estado de validación

La compilación real de Windows está completada y dist/2.0.0/Contador.exe fue reemplazado por la revisión corregida. El motor conserva su verificación de 285 comprobaciones y seis del OCR real con imágenes generadas. La revisión de experiencia incorpora 13 comprobaciones del panel y cuatro de cierre/cuentas, con capturas en tamaño normal y reducido. Las pruebas no capturaron la pantalla del usuario. Falta contrastar el resultado en una partida grabada y contada manualmente: no se acredita exactitud ni detección instantánea.

La revisión admite etiquetas de cofres raros, enterrados y de espíritu, variantes españolas, XP unido a la misma línea y varios tipos en un aviso. Alterna color original y máscara blanca/violeta, procesa fuera del hilo de interfaz y reduce la espera entre lecturas. Una ventana de tres lecturas requiere dos observaciones que respalden una cantidad, aunque el filtro intermedio pierda el multiplicador. El diagnóstico indica si reconoce el aviso y el tiempo de lectura. No guarda texto ni imágenes en archivos.

## Flujo y privacidad

El botón dorado **Activar conteo en tiempo real** abre una pregunta con Sí/No y No como opción inicial. Rechazar no crea el lector ni captura nada: la detección por repeticiones continúa. El permiso vale solo para esa ejecución y puede retirarse con **Desactivar conteo**. Al cerrar se cancela la sesión; al volver a abrir comienza desactivado.

El panel distingue **Desactivado**, **Esperando partida**, **Contando**, **En pausa** y un fallo que necesita atención. Al desactivarlo se ocultan el contador vacío y los detalles técnicos. **Detalles del conteo** permite consultar el diagnóstico cuando está activado, sin mostrarlo en la vista principal. Los jugadores solo aparecen si hay datos, sin una fila duplicada del contador propio.

Solo se copian dos regiones del HUD de la ventana de Fortnite en primer plano: avisos de experiencia y nombres del equipo. El proceso se verifica por su nombre; no se leen su memoria ni archivos de credenciales. La captura se pausa fuera de una partida detectada, con la detección pausada o al perder el foco. No se capturan audio ni monitores completos. Píxeles, texto y nombres leídos permanecen en memoria; no se guardan imágenes, no se escriben en el registro de errores y no se envían a una API.

Se utiliza el reconocimiento de texto instalado en Windows, mediante Windows.Media.Ocr. Requiere Windows 10 2004 o posterior, un idioma de OCR instalado y Fortnite en ventana sin bordes. Las regiones relativas del HUD pueden no coincidir con un HUD personalizado o una escala distinta: no se garantiza detección. Un fallo de captura u OCR deja operativo el análisis por repeticiones.

## Qué significa cada cifra

- **Historial confirmado:** registros de repeticiones finalizadas; no incluye estimaciones visuales.
- **≈ cofres · estimación visual:** avisos de tipos CHEST/COFRE acompañados de XP/PE. Exige dos observaciones compatibles entre las tres últimas lecturas válidas y espera cuatro lecturas vacías antes de rearmar un aviso. Suma tipos distintos sin duplicar líneas y los avisos apilados solo añaden la diferencia. Estas reglas reducen duplicados, pero no acreditan exactitud. El aumento sigue al aviso de experiencia, no a la animación de abrir el cofre.
- **Nombres del HUD:** etiquetas sin verificar. No se utilizan como Epic ID, para conciliar fuentes, ni para atribuir una apertura. El HUD puede incluir tu propio nombre o texto mal reconocido.
- **Compañeros durante la partida:** sin cifra en vivo. La pantalla de un jugador no muestra todas las aperturas de los demás.
- **Última partida finalizada:** nombres y mínimos por compañero obtenidos del replay. Se identifica expresamente como última partida para no confundirlo con el equipo actual.

Los cofres destruidos sin aviso, avisos fuera de la región, OCR impreciso, espectar a otro jugador y cambios de HUD pueden omitir o atribuir mal una detección. Activar a mitad de partida solo observa lo que ocurra después. Al terminar, el análisis de la repetición actualiza el historial independientemente; nunca se suman la estimación y el resultado final.

## Referencia y licencia

Se estudió [Inceptofn/Fortnite-Chest-Tracker](https://github.com/Inceptofn/Fortnite-Chest-Tracker/tree/c21c59a963f049876fe3c50d5b481c28c91df684), especialmente src/ChestCounter.Ocr/LiveChestCounter.cs y la presentación de equipo de src/ChestCounter.Gui/MainWindow.xaml.cs. El enfoque de avisos XP, regiones pequeñas y deduplicación temporal se adapta con permiso explícito, restricción al proceso de Fortnite en primer plano y separación del historial. Su afirmación de seguridad frente al antitrampas no se adopta como garantía. Aviso MIT conservado en vendor/LICENSE-Inceptofn-Live.txt.

La biblioteca OCR es de Windows: [documentación de OcrEngine](https://learn.microsoft.com/uwp/api/windows.media.ocr.ocrengine). No se incorporan servidores de sincronización, automatización del juego ni los componentes de actualización del proyecto de referencia.
