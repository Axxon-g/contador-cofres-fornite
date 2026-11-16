# Contador
<!-- impeccable:product-schema 1 -->
## Platform
Windows desktop
## Stack
C#/.NET, aprobado por el usuario. WPF para la ventana; lector original conservado.
## Users
Jugadores de Fortnite en Windows que quieren contar sus cofres.
## Product Purpose
Contar cofres desde repeticiones finalizadas, con una interfaz moderna y limpia.
## Capabilities and Constraints
Repeticiones en solo lectura, recuento por cuenta, historial local y overlay. El recuento depende del formato soportado por el lector original.

## Equipo y procedencia
Espacio Equipo dedicado a reunir repeticiones de compañeros de una misma partida, consultar conteos por jugador, origen y cobertura y retirar fuentes importadas. La reconciliación conserva la mejor cifra disponible por jugador sin sumar repetidamente perspectivas de la misma partida. Las fuentes se administran localmente y retirarlas no elimina sus archivos ni el historial personal.

Los conteos de equipo son mínimos registrados: una o varias repeticiones pueden omitir acciones, incluso de compañeros. No se garantiza exactitud perfecta en partida. Las estimaciones visuales opcionales no se incluyen en la reconciliación; las atribuciones estimadas se identifican y no alteran las cifras propias. La compatibilidad sigue limitada por el lector y el formato de repetición soportado.

## Organización de la aplicación
Cuenta global y siete espacios nativos: Resumen, Estadísticas, Historial, Equipo, Mapa y rutas, Informe y Ajustes. La moneda y el dorado siguen siendo identidad aprobada, con rail rojo pirata, fondos marinos, acentos turquesa/coral y navegación compacta.

## Estadísticas públicas mediante AXXON
Estadísticas, debajo de Resumen, consulta victorias, partidas, eliminaciones, K/D, porcentaje de victorias, nivel del pase actual y modalidades de Battle Royale. La búsqueda manual por nombre Epic, PlayStation o Xbox es inicial; usar el perfil local seleccionado es opcional, con Epic ID válido o nombre. Filtros: historial/temporada y tipo de control. Tríos no está disponible; raya significa dato ausente. Los datos públicos son independientes de cofres, recompensas e historial local; las fechas del contador no se aplican.

La app consulta únicamente https://www.axxon.dev/api/fortnite-stats sin Authorization ni claves del proveedor. El usuario no introduce claves ni contraseñas. Se rechazó el diseño anterior de claves por usuario: no hay almacén de credenciales ni configuración de clave en el cliente. El servicio compartido conserva FORTNITE_API_KEY exclusivamente como variable privada de Vercel y usa Fortnite-API como proveedor comunitario. No se envían repeticiones, cofres ni capturas; solo objetivo de búsqueda y filtros.

El servicio requiere clave y Upstash Redis para caché, límites y cuotas; sin configuración responde 503 sin consultar al proveedor. El servicio está desplegado y se confirmó una respuesta real en su dominio definitivo. La app limita respuesta a 256 KiB, timeout a quince segundos y caché de memoria a cinco minutos. Perfiles privados, indisponibilidad y límites se explican mediante estados textuales y acciones de recuperación. Las pruebas automatizadas usan respuestas simuladas; la comprobación del servicio real no sustituye una prueba completa de la app con cada perfil.
