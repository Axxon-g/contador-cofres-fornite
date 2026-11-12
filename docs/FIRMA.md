# Firma digital y SmartScreen

El ejecutable actual no esta firmado. La advertencia «Editor desconocido» indica que Windows no puede identificar un editor verificado; este proyecto no puede eliminarla cambiando el nombre o el icono del archivo.

Para distribuir con un editor identificado necesitas obtener un certificado de firma de codigo de una entidad de confianza, con verificacion de identidad. No se incluye ninguna clave privada en el repositorio. Los certificados autofirmados no resuelven la confianza publica.

## Publicar con un certificado

1. Configura el certificado y su clave privada en `Cert:\CurrentUser\My` siguiendo las instrucciones de tu proveedor (incluido su token o dispositivo, si corresponde).
2. Consulta su huella desde PowerShell con `Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert`.
3. Ejecuta `build.ps1` con `-SigningCertificateThumbprint` seguido de la huella y `-TimestampServer` seguido de la URL de sellado de tiempo indicada por tu proveedor.

El proceso compila primero, firma una copia del EXE final con SHA256, verifica la firma y el sello de tiempo y solo entonces reemplaza el ejecutable. Si falla, no publica la copia como firmada. No exporta claves ni instala certificados. La integracion admite certificados accesibles mediante PowerShell; los servicios de firma remota pueden necesitar su propia herramienta.

Para comprobar los requisitos sin firmar, ejecuta `tools/Sign-Release.ps1` con los parametros `-Executable`, `-CertificateThumbprint`, `-TimestampServer` y `-ValidateOnly`.

Sube a Releases el EXE final y su archivo `Contador.exe.sha256`, generado despues de firmar. No modifiques el ejecutable despues de la firma. La suma permite comprobar integridad; no sustituye la firma ni elimina SmartScreen.

## Limites

Una firma valida identifica al editor, pero una version nueva puede seguir mostrando SmartScreen mientras acumula reputacion. No existe una modificacion del codigo que garantice quitar el aviso. No pidas a los usuarios desactivar Defender ni instalar un certificado raiz del proyecto.

Otra via es preparar un paquete MSIX para Microsoft Store, sujeto a registro, certificacion y aprobacion. No se ha empaquetado ni publicado en Store como parte de este cambio.

Referencias: [SmartScreen y reputacion](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/smartscreen-reputation), [opciones de firma](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options) y [firma con PowerShell](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.security/set-authenticodesignature).
