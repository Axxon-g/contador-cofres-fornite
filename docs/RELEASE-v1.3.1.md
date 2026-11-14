# Contador 1.3.1 — Sigue contando en segundo plano

## Nuevo: bandeja de Windows

- La moneda de Contador aparece junto al reloj, dentro de los iconos visibles u ocultos de Windows.
- Un clic abre la ventana; un clic derecho permite elegir **Abrir Contador** o **Salir**.
- Al pulsar la X puedes elegir **Mantener en segundo plano**, **Salir de Contador** o **Cancelar**.
- En segundo plano continúan la detección de partidas, el recuento y el overlay, sin abrir otra instancia.
- Al salir se espera el análisis pendiente y se ejecuta el guardado habitual antes de cerrar.
- Si Windows no permite crear el icono, la app mantiene accesible su ventana y deshabilita la opción de ocultarla.
- El icono se retira al salir y se intenta registrar de nuevo si se reinicia el Explorador de Windows.

## Cómo actualizar

1. En el contador anterior, elige **Salir de Contador** o pulsa clic derecho en la moneda de la bandeja y selecciona **Salir**. Mantenerlo en segundo plano no basta para actualizarlo.
2. Espera a que termine de guardar y sustituye el ejecutable anterior por **Contador.exe** de esta Release.
3. Abre el nuevo archivo y comprueba que la esquina de la ventana muestra **v1.3.1**.

Se conservan tu historial, ajustes y rutas. No necesitas cuenta, claves ni instalar .NET para utilizar el ejecutable autónomo Windows x64.

El archivo **Contador.exe.sha256** es opcional: sirve para comprobar la integridad de la descarga.

## Se mantienen las funciones de la 1.3

Mapa HD activado por defecto y configurable, zoom hasta 3200 %, caché local, rutas editables, informe nativo, perfiles, filtro de fechas y overlays.

El recuento de cofres sigue dependiendo de repeticiones locales finalizadas. Las ubicaciones comunitarias no garantizan premios ni apariciones. El HD depende del proveedor externo y conserva el mapa integrado como respaldo. El ejecutable no tiene firma digital de editor verificado; un análisis de VirusTotal de otra compilación no verifica este archivo.
