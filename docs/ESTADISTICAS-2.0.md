# Estadísticas compartidas · Contador 2.0

Esta revisión retira por completo los campos, almacenamiento y llamadas de claves de API por usuario. No hay clave del proveedor en el ejecutable.

Estadísticas permanece debajo de Resumen. El usuario escribe un nombre, elige Epic Games/PlayStation/Xbox y consulta. También puede usar la cuenta local seleccionada por su ID disponible; tras buscarla, los cambios de perfil actualizan esa consulta. El historial de cofres permanece separado.

El destino es fijo: https://www.axxon.dev/api/fortnite-stats. Se consulta directamente el dominio definitivo para evitar el salto desde axxon.dev, ya que el cliente no sigue redirecciones. El cliente no envía Authorization ni contraseñas. La clave solo la configura el autor en Vercel. El servicio compartido está implementado en el repositorio axx_port_2026, con caché y límites distribuidos de Redis.

No se recuperan cofres a través de esta API. Los perfiles privados pueden rechazar consultas, Tríos no está disponible y los valores ausentes se muestran como —. Los filtros de fecha de los replays no afectan a las estadísticas.

El cliente utiliza HTTPS, tiempo de espera de 15 segundos, respuestas de hasta 256 KiB, caché temporal y cancelación de respuestas obsoletas. No publica búsquedas ni modifica registros de partidas.

La integración anterior de claves locales fue retirada. Si llegaste a guardar una clave con esa versión de prueba, esta revisión no la lee; puedes eliminar la entrada ContadorModern/FortniteApi en el Administrador de credenciales de Windows.

El servicio está desplegado. Se confirmó una respuesta real de Axxon_G en el dominio definitivo, sin acceder a la clave privada. Las pruebas automatizadas del cliente usan respuestas simuladas y comprueban el destino canónico, los errores, la caché y la ausencia de cabeceras de autorización.
