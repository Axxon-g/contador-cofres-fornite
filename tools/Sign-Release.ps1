[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Executable,
    [Parameter(Mandatory)][string]$CertificateThumbprint,
    [Parameter(Mandatory)][uri]$TimestampServer,
    [switch]$ValidateOnly
)
$ErrorActionPreference = 'Stop'
$thumbprint = ($CertificateThumbprint -replace '\s', '').ToUpperInvariant()
if ($thumbprint -notmatch '^[A-F0-9]{40}$') { throw 'La huella debe contener 40 caracteres hexadecimales.' }
$certificate = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert | Where-Object Thumbprint -eq $thumbprint | Select-Object -First 1
if (-not $certificate) { throw 'No se encontro el certificado de firma con clave privada en Cert:\CurrentUser\My. Obtiene primero un certificado de editor verificado.' }
$now = Get-Date
if ($now -lt $certificate.NotBefore -or $now -ge $certificate.NotAfter) { throw 'El certificado no esta vigente.' }
if ($certificate.Subject -eq $certificate.Issuer) { throw 'No se admite un certificado autofirmado para publicar.' }
$chain = New-Object System.Security.Cryptography.X509Certificates.X509Chain
try {
    $chain.ChainPolicy.ApplicationPolicy.Add((New-Object System.Security.Cryptography.Oid '1.3.6.1.5.5.7.3.3'))
    if (-not $chain.Build($certificate)) { throw 'Windows no pudo validar la confianza o revocacion del certificado. Revisa el certificado y la conexion.' }
} finally { $chain.Dispose() }
if ($TimestampServer.Scheme -notin @('http','https')) { throw 'Usa la URL HTTP/HTTPS de sellado de tiempo indicada por tu proveedor.' }
$target = (Resolve-Path -LiteralPath $Executable).Path
if ([IO.Path]::GetExtension($target) -ne '.exe') { throw 'Selecciona el ejecutable final .exe.' }
if ($ValidateOnly) { Write-Host 'Certificado y archivo validados; no se modifico el ejecutable.'; return }
# Firmar una copia evita reemplazar el original cuando falle el servicio o la validacion.
$staging = Join-Path ([IO.Path]::GetDirectoryName($target)) ([guid]::NewGuid().ToString() + '.exe')
try {
    Copy-Item -LiteralPath $target -Destination $staging
    $signature = Set-AuthenticodeSignature -LiteralPath $staging -Certificate $certificate -HashAlgorithm SHA256 -IncludeChain NotRoot -TimestampServer $TimestampServer.AbsoluteUri
    $signature = Get-AuthenticodeSignature -LiteralPath $staging
    if ($signature.Status -ne 'Valid' -or -not $signature.TimeStamperCertificate) { throw 'No se obtuvo una firma valida con sello de tiempo; se conserva el original.' }
    Move-Item -LiteralPath $staging -Destination $target -Force
    Write-Host 'Firma digital y sello de tiempo verificados. SmartScreen puede seguir mostrando avisos hasta acumular reputacion.'
} finally { if (Test-Path -LiteralPath $staging) { Remove-Item -LiteralPath $staging } }
