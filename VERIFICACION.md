# Contador 2.2 · verificación previa a publicación

- Motor: **365 comprobaciones** correctas.
- Escritorio: **93 comprobaciones** correctas: cierre/cuentas (4), mapa (18), instancia (8), segundo plano (8), equipo (3), conteo visual (13), pantallas (20), perfil público (10) y tesoros (9).
- Total: **458**. Las comprobaciones usan datos de prueba y no activan capturas del usuario.
- Compilación Release del motor y escritorio completada. Las vistas grande y compacta se revisaron durante el rediseño.
- Ejecutable final verificado por SHA-256; la huella y las instrucciones para adjuntarlo están en [RELEASE-v2.2.md](docs/RELEASE-v2.2.md).
- Revisión de patrones de credenciales en cambios publicables sin coincidencias. Los registros, repeticiones, credenciales y duplicados locales se excluyen de Git.
- Los avisos heredados del lector no son errores de compilación. La consulta de vulnerabilidades de NuGet no estuvo disponible durante la publicación previa; no se afirma una auditoría actualizada de paquetes.
- Sigue pendiente validar el replay afectado reportado por otro usuario y varias perspectivas del mismo equipo contra un recuento manual. No se garantiza exactitud completa ni compatibilidad con todo formato de Fortnite.

---

## Registro histórico de validaciones

# Claridad del conteo en tiempo real · revisión de experiencia

- 13 comprobaciones del panel y cuatro de cierre/cuentas aprobadas con el capturador real compilado, sin activar capturas del usuario.
- Estados explícitos Desactivado, Esperando partida, Contando, En pausa y Necesita atención. La acción solicita permiso; pulsar el control por sí solo no activa la captura.
- Botón dorado de activación, acción secundaria de desactivación, foco de teclado y ausencia de cifras vacías cuando está apagado. Diagnóstico técnico cerrado por defecto y oculto al desactivar.
- Capturas revisadas a 780 y 520 unidades. Se corrigió el recorte del subtítulo de la cifra y se eliminaron las filas duplicadas/vacías del contador propio.
- Compilación x64 completada y dist/2.0.0/Contador.exe reemplazado; copia verificada por SHA-256. El archivo Contador_test.rar del usuario se conserva sin modificar.

---

# Corrección de lectura visual de 2.0.0 · verificación del motor

- Motor: 277 comprobaciones aprobadas, con variantes españolas/inglesas, tipos apilados, multiplicadores parcialmente leídos y atribución tardía de cofres destruidos.
- OCR real de Windows: seis comprobaciones sobre imágenes generadas blancas/violetas, con color y filtro. Se reprodujo la pérdida del multiplicador en una variante y se verificó la conciliación de lecturas. No se capturó la pantalla del usuario.
- Interfaz real compilada con Windows SDK: dos comprobaciones del panel y cuatro de cierre/cuentas aprobadas. Renderizado a 780 y 520 unidades sin sustituto del capturador.
- Dos repeticiones locales finalizadas contienen respectivamente 11 cofres, cuatro destruidos, y 39 cofres, 30 destruidos. Esto comprueba que se registran destrucciones, no que estén todas las del juego.
- Publicación autocontenida x64 completada y copia comprobada por SHA-256. dist/2.0.0 contiene solo el ejecutable corregido, versión 2.0.0.0. El EXE y RAR anteriores se preservaron fuera de dist como respaldo de trabajo.
- Pendiente una prueba en partida con referencia manual. No se promete detección de roturas sin aviso XP ni exactitud completa.

---

# Registro anterior · desarrollo de lectura visual

- 264 comprobaciones del motor aprobadas en una copia aislada, incluidas 14 de avisos XP/PE, duplicación, intervalos y separación de cifras visuales.
- La interfaz se compiló y renderizó a 780 y 520 unidades con un sustituto de captura que nunca lee la pantalla. Dos comprobaciones verifican estado inicial apagado e historial separado. Esto no acredita el capturador real.
- La descarga de Microsoft.Windows.SDK.NET.Ref quedó bloqueada en el entorno. No se completaron la compilación real del OCR, la prueba de permiso en partida ni la calibración con capturas reales.
- dist/2.0.0/Contador.exe permanece en su compilación anterior, verificada a continuación. No se publicó un ejecutable con el OCR sin validar.

---

# Verificación de 2.0.0 · 14/11/2026

- Publicación autocontenida Windows x64 completada. dist/2.0.0 contiene únicamente Contador.exe; versión de archivo 2.0.0.0. Copia comprobada mediante SHA-256. No se abrió la aplicación sobre el historial real; la interfaz se renderizó con datos ficticios.

- Motor: 250 comprobaciones aprobadas, incluidas 25 del registro de interacciones y conciliación de fuentes.
- Escritorio: 4 de cierre/cuentas, 3 de equipo, 18 de mapa, 8 de instancia única y 8 de segundo plano aprobadas.
- Tres controles reales de solo lectura conservan los cofres personales: 11, 39 y 35. El mínimo de compañeros se mantiene en 6 y cambia de 21 a 22 y de 17 a 25. La diferencia de rellenos por campos replicados omitidos se reprodujo y corrigió; no se modificó el historial real.
- La conciliación multiperspectiva se validó con casos sintéticos de duplicación, sesión ajena, enemigo, identidad propia, versión antigua, identidad ausente y conflicto. Falta una validación de campo con archivos de varios compañeros de una misma partida y conteo manual.
- Las pruebas no garantizan exactitud completa del juego ni ausencia universal de fallos. El equipo sigue siendo experimental; no se adoptaron inferencias espaciales como cifras personales confirmadas.
- Investigación y decisiones: docs/INVESTIGACION-v2.0.md. No se ejecutaron binarios de los proyectos comparados.

---
# Verificación de 1.3.2 · 14/11/2026

- Motor: 225 comprobaciones aprobadas, incluyendo seguridad de lectura, CSV, equipo y publicación estable del overlay.
- Escritorio: 4 de cierre/cuentas, 3 de equipo, 18 de mapa, 8 de instancia única y 8 de segundo plano aprobadas.
- Compilación Release del motor y escritorio sin errores. Persiste una advertencia de nulabilidad heredada en ReplayAnalyzer al pasar un logger opcional.
- Se excluyen de la compilación el actualizador, el lanzador de espera, la consola nativa y la bandeja heredados; sus fuentes originales se conservan en vendor.
- El conteo grupal continúa siendo experimental y parcial. Las pruebas no acreditan exactitud completa, ausencia de vulnerabilidades ni compatibilidad futura con Fortnite.

Los registros anteriores se conservan a continuación como historial de verificación.

---
# Verificación de Contador

## Segundo plano y bandeja · 1.3.1

- Ocho comprobaciones del flujo de segundo plano: cancelar, ocultar sin guardar ni detener, dispatcher activo mientras está oculto, restauración, ocultación deshabilitada si no hay icono y salida explícita sin repetir la pregunta ni el guardado.
- Tres comprobaciones de cierre: inmediato, análisis pendiente y error de guardado.
- Las pruebas nativas de registro de icono fueron bloqueadas por Win32 5 en el entorno de ejecución. Las capturas reales aportadas por el usuario muestran la moneda, el menú Abrir Contador/Salir y la elección al cerrar funcionando en su escritorio. No se ha simulado reiniciar su Explorador ni cerrar la sesión de Windows.
- Se conserva el bloqueo de instancia única y no se activa el inicio automático con Windows.
- Los resultados de estas pruebas no acreditan la suite general de almacenamiento, previamente bloqueada por permisos del entorno.

## Versión del ejecutable · 1.3.0

Proyecto actualizado a 1.3.0. La etiqueta de la ventana y el título del mapa usan AppVersion, derivada de la versión del ensamblado, en lugar de literales separados. Se comprobó por reflexión la etiqueta v1.3 y la versión 1.3.0.0; publicación autocontenida win-x64 completada. Pasaron 18 pruebas WPF y ocho de instancia única. La salida de esta versión está en dist/1.3.0/Contador.exe; el ejecutable histórico de dist/1.2.7 sigue siendo una compilación anterior.

## HD activado por defecto · 1.2.7

Por petición del usuario, Terreno HD inicia activado. Se mantienen el control para desactivarlo y el respaldo integrado. Las 18 comprobaciones WPF pasan con la nueva condición inicial y verifican desactivación y reactivación. Las pruebas y capturas aisladas bloquean la red para evitar descargas durante su ejecución. Esta modificación sustituye la condición inicial apagada documentada en las verificaciones históricas siguientes.

## Inicio e instancia única · 1.2.7

Ocho pruebas aisladas de mutex y señal de activación aprobadas: objeto existente sin propietario, comprobación repetida, liberación al salir, instancia realmente ocupada, activación de la principal, secundarias que no liberan el bloqueo ajeno y recuperación tras terminar el hilo propietario. Dieciocho pruebas del mapa aprobadas. El cierre y guardado general volvió a quedar bloqueado por permisos File.Move del entorno, por lo que no se declara aprobado. No se ejecutó la aplicación completa sobre el historial personal para esta prueba.

El arranque comprueba propiedad efectiva del mutex, libera explícitamente su bloqueo al salir y controla los errores al crear la ventana para evitar permanecer abierto sin ventana. Un segundo inicio notifica a la instancia existente para restaurarla; también intenta restaurar la ventana de versiones anteriores. No termina procesos ajenos ni ignora un bloqueo que siga teniendo propietario.

## Terreno HD opcional · 1.2.7

48 comprobaciones del mapa, 18 de controles WPF y cuatro de caché con una pieza WebP real aprobadas. Construcción nativa correcta. La opción HD inicia apagada, dispone de atribución, zoom hasta 3200 %, carga por niveles y respaldo integrado. Capturas reales en el espacio de la tarea: outputs/mapa-hd/renders; la captura HD utiliza una sola pieza local y muestra cobertura incompleta. La revisión visual aprueba esta extensión opcional, sin certificar la disponibilidad del servidor externo. No se validó una sesión HD completa por Internet. Procedencia y límites en docs/MAPA-HD-1.2.7.md.

- Compilación Windows x64 autónoma: ejecutable dist/1.2.5/Contador.exe.
- 35 comprobaciones correctas con una copia del historial real: cabeceras, perfiles, persistencia, importación, respaldo, detección de partida, retirada del aviso al finalizar y exclusión de repeticiones abandonadas.
- Sesión comprobada con historial anterior, importaciones, repeticiones antiguas y varias partidas nuevas. Última partida y sesión se calculan por separado.
- Capturas nativas del overlay en espera y en partida, con etiquetas de última partida, sesión y tesoros.
- Launcher renderizado en tamaño normal y compacto, historial y ajustes. Moneda aportada por el usuario embebida como recurso WPF.
- Revisión visual independiente: ship para el código y las capturas; sin hallazgos materiales.
- La detección se basa en cabeceras de repeticiones escritas recientemente por Fortnite y se consulta cada dos segundos. No se validó durante una partida real en esta sesión de trabajo. El recuento final sigue dependiendo de terminar la repetición.
- Persisten avisos heredados y de consulta de vulnerabilidades por falta de acceso a nuget.org; no se alteró el analizador de cofres.

## Actualización 1.2.1

La moneda se usa como icono multirresolución del ejecutable y ventana (16–256 px) y reemplaza el cofre del overlay mediante dibujo nativo con transparencia. Todos los recursos están embebidos. Pasaron 43 comprobaciones en el modo de captura, incluidas las siete resoluciones del ICO, el dibujo real de la moneda y capturas de ambos estados del overlay. El README documenta procesamiento local y ausencia de inicio de sesión o credenciales; no afirma aprobación de Epic ni inmunidad a sanciones.

## Cierre corregido en 1.2.2

Tres pruebas con ventanas WPF nativas pasaron: cierre inmediato, cierre mientras hay un análisis pendiente (incluyendo clics repetidos) y fallo de guardado. El controlador aplaza la persistencia y el cierre final mediante el Dispatcher, de modo que nunca vuelve a llamar Close ni muestra diálogos dentro del primer evento Closing.

## Overlay unificado en 1.2.3

La fuente de OBS utiliza ObsHud y la misma vista inmutable que NativeOverlay. Pasaron 46 comprobaciones del motor/HUD, incluidas transparencia, moneda, etiquetas, datos locales, estado de partida y protección frente a la regeneración del informe. Se ejecutó también el JavaScript generado con un documento simulado: actualizó cifras y estado, y solicitó exclusivamente el archivo local cada dos segundos.

No se verificó visualmente dentro de OBS. Los navegadores de pruebas locales fallaron al iniciarse y la herramienta de navegador bloqueó la apertura de archivos locales por política. La compilación y las pruebas de generación/actualización pasaron; queda comprobar la apariencia en la fuente Navegador de OBS.

## Reporte renovado en 1.2.4

Se actualizó el CSS del reporte a la paleta y tipografía del launcher y se integró la moneda en una URI de datos. Se añadieron comprobaciones de colores, imagen embebida, ausencia del tema claro anterior y conservación de gráficos y tablas. La herramienta de navegador de esta sesión no permite abrir archivos locales; no se certifica una inspección visual del reporte en ese navegador.

## Actualización rápida en 1.2.5

Pasaron 63 comprobaciones del motor y 3 pruebas WPF de cierre. Las pruebas deterministas verifican dos snapshots idénticos y un segundo de estabilidad, reinicio de espera ante cambios de tamaño o fecha y rechazo de confirmación si el archivo cambia o desaparece durante el análisis.

El sondeo pasa de ocho a un segundo. Launcher y overlays reciben las cifras antes de generar el informe; se elimina el informe repetido sin cambios y la compactación forzada previa a publicar resultados. No se midió la latencia de extremo a extremo durante una partida real en esta sesión; no se promete un tiempo fijo de actualización.

## Corrección de cuentas · 1.2.6

La prueba WPF utiliza un ComboBox real con dos cuentas de igual nombre y distintos Epic ID. Mantiene ocupado el bloqueo del analizador, elige la segunda cuenta, simula una actualización y comprueba que la elección pendiente se conserva. Tras liberar el bloqueo verifica el perfil guardado y el total separado; también permite volver al primero y desactiva la lista vacía. Pasaron las cuatro comprobaciones WPF y las 61 comprobaciones básicas del motor. No se ha validado todavía con las partidas de los usuarios que reportaron el problema.

## Período por fechas · 1.2.7

Pasaron 77 comprobaciones del motor y las cuatro comprobaciones WPF. Las nuevas pruebas cubren inicio y fin inclusivos, conversión UTC a fecha local, UTC heredada sin Kind, fecha de análisis como respaldo, rango invertido, configuración anterior sin filtro, persistencia, cuenta separada, perfiles sin partidas, historial conservado y coherencia del CSV, informe y overlay. Los calendarios y el mensaje de error se revisaron visualmente con XAML renderizado a 1120×740 y 860×600; las capturas usan estado de demostración. No se ha validado el filtro con historiales de los usuarios que reportaron el problema.

## Mapa interactivo e informe nativo · compilación 1.2.7

- Publicación autocontenida Windows x64 completada, con mapa y catálogo integrados.
- 40 comprobaciones del mapa aprobadas: catálogo, coordenadas, agrupación sin pérdida ni colisiones, límites de zoom, edición, deshacer y formato del plan.
- 15 comprobaciones de controles WPF aprobadas.
- Once vistas WPF renderizadas; la selección del README procede de esas capturas. El informe utiliza datos ficticios identificados.
- Revisión final de dos ajustes: separación de grupos y etiqueta del total filtrado, ambos resueltos.
- La suite general de almacenamiento y las pruebas anteriores de perfil quedaron bloqueadas por UnauthorizedAccess del entorno en esa ejecución; no se declaran aprobadas.
- No se validaron todas las ubicaciones comunitarias en partidas reales. Son posibles apariciones de una instantánea 43.10, no detección de botín ni predicción.
- La huella debe recalcularse tras cada compilación o firma. Usa el archivo Contador.exe.sha256 que acompaña al ejecutable, no una huella histórica del README.

## Corrección de bóvedas · 1.2.7

43 comprobaciones del mapa y 15 de controles WPF aprobadas. Doce capturas nativas generadas, incluida capa aislada de nueve bóvedas. Alineación visual contrastada con la referencia aportada y grupos independientes de cofres raros. Corregida recursión de notificaciones de selección detectada durante esta ejecución. No se certifica ubicación de entradas ni precisión de las demás capas en una partida real.

## Renovación del launcher y conciliación de equipo · 15/11/2026

- 285 comprobaciones del motor aprobadas: selección de la mejor perspectiva por jugador, sesión/identidad verificadas, duplicados, conflictos y preservación del registro personal.
- 16 comprobaciones del conjunto de pantallas, 13 del panel de conteo opcional, 3 del equipo, 18 del mapa y 4 de cierre/cuentas: 339 en total con el motor.
- Seis pantallas capturadas a 1180 y 860 unidades con datos ficticios identificados, sin capturas reales ni acceso de red del mapa. Verificación de fechas es-MX y selección/limpieza del campo.
- Publicación autocontenida Windows x64 correcta, versión 2.0.0. No se ejecutó el capturador contra una partida real.
- Pendiente validación de campo: comparar archivos de varios compañeros de una misma partida con un conteo manual. Los datos de equipo siguen siendo mínimos y experimentales.

## Preparación de 2.0.0 para publicación · 15/11/2026

El script build.ps1 se verificó con compilación del motor y escritorio, seguida de 285 comprobaciones del motor, 4 de cierre/cuentas, 18 del mapa, 8 de instancia única, 8 de bandeja, 3 del equipo, 13 del conteo opcional y 16 del conjunto de pantallas/fechas: **355 aprobadas**. Las pruebas utilizan datos de prueba; no capturan una partida real.

README y notas de versión usan el flujo actual de Equipo y la cuenta de cabecera. Incluyen capturas aportadas del uso real y una vista ficticia identificada para explicar la conciliación de fuentes. Los enlaces e imágenes locales del README se comprobaron.

El formato de los archivos C# modificados de src/tests se normalizó con dotnet format en modo carpeta; los proyectos XML se separaron en líneas legibles. No se reescribieron bibliotecas heredadas solo por estilo. Se añade .editorconfig para conservar el formato.

La compilación conserva advertencias del lector heredado. NuGet emitió NU1900 porque este entorno no pudo consultar sus datos de vulnerabilidades por falta de conexión; no se desactivó esa comprobación ni se presenta como una auditoría aprobada de dependencias.

El script compila cada proyecto de pruebas una vez, ejecuta los escenarios sin reconstruirlos y publica en staging antes de reemplazar únicamente Contador.exe. Ya no elimina archivos ajenos de dist.

La primera copia final fue bloqueada por la app abierta. Tras cerrarla, se reemplazó dist/2.0.0/Contador.exe y se verificó su igualdad SHA-256 con staging; se preservaron los archivos adicionales del usuario. El ejecutable queda fuera de Git y se adjunta a la Release.


## Consulta mediante servicio compartido

Se retiró la variante de claves por usuario y su almacén de credenciales. El cliente utiliza https://axxon.dev/api/fortnite-stats, sin Authorization ni valores privados. Compilación de la aplicación y publicación autocontenida verificadas en una copia de trabajo; 301 comprobaciones del motor, 9 del panel público y 18 del conjunto de pantallas aprobadas. Las pruebas usan datos ficticios y no una clave real.

El servicio web supera 11 pruebas automatizadas de validación, normalización, ausencia de credenciales en respuestas, caché, límites y fallo cerrado. Las comprobaciones estáticas de seis páginas, Contador y descargas también pasan. La instalación de dependencias Vite quedó bloqueada por ENOTFOUND del registro npm y la revisión visual del sitio no pudo completarse por bloqueo de la vista previa/navegador. No se afirma que esa compilación web ni el despliegue hayan sido verificados.

El ejecutable nuevo se publica en la carpeta de trabajo. La copia a dist fue denegada por el entorno; el archivo anterior de dist no se considera actualizado con esta revisión. Debe desplegarse/configurarse el servicio y verificarse una consulta real antes de distribuir la nueva compilación.

## Corrección de estadísticas: dominio definitivo

El cliente de Contador usa directamente https://www.axxon.dev/api/fortnite-stats. La dirección anterior sin www redirige a ese dominio; el cliente mantiene desactivadas las redirecciones automáticas. Se confirmó en navegador una respuesta JSON real de Axxon_G en el destino definitivo, sin leer la clave privada.

Validación: 302 comprobaciones del motor y 9 del panel WPF de estadísticas aprobadas. Publicación Windows x64 generada con la preparación local de compilación existente; se verificó que todas las fuentes C#, XAML y proyectos de src/vendor coincidieran con el repositorio. Se reemplazó dist/2.0.0/Contador.exe y se comprobó su SHA-256 contra la compilación. La ejecución completa de la nueva app con una búsqueda real queda para la prueba del usuario; la comprobación de producción se hizo sobre el servicio y las pruebas del cliente usan respuestas simuladas.

## Preparación 2.0.1

Versión del proyecto, etiqueta derivada del ensamblado y ejecutable de distribución alineados a 2.0.1. Archivo: dist/2.0.1/Contador.exe; FileVersion 2.0.1.0 y ProductVersion 2.0.1. SHA-256: F16B7BB68ADD03DC50D4B8199E03B5AC248601ABF9D896E78C59FA8A57C0B679.

Pruebas: motor 302; cierre/cuentas 4; mapa 18; instancia única 8; bandeja 8; equipo 3; conteo opcional 13; shell 18; estadísticas 9. Total 383 aprobadas. Publicación autocontenida Windows x64 y coincidencia de la copia comprobadas. La consulta de vulnerabilidades de NuGet no estuvo disponible por la conexión del entorno; no se presenta como una comprobación aprobada de dependencias.

Revisión de publicación: integración nueva y flujos revisados estáticamente, sin hallazgos de fuga en los cambios examinados. Búsqueda de patrones de secretos sobre 354 textos actuales y 549 blobs históricos locales; la única coincidencia fue un GUID de sesión en una prueba, no una credencial. Ningún .env, certificado privado, clave o replay figura entre los archivos publicables. Se añadieron exclusiones preventivas de esos archivos y de registros/datos locales. No se inspeccionaron todos los binarios/imágenes ni todas las dependencias; no se garantiza ausencia absoluta de vulnerabilidades. La revisión cubre los cambios de la integración previa al ajuste de metadatos/documentación de 2.0.1.
