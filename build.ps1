param([string]$SigningCertificateThumbprint, [uri]$TimestampServer)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Instala el SDK de .NET 10 para Windows x64 desde https://dotnet.microsoft.com/download/dotnet/10.0' }
$sdkVersions = & dotnet --list-sdks
if (-not ($sdkVersions -match '^10\.')) { throw 'Se necesita el SDK de .NET 10. El runtime de .NET no basta para compilar.' }
& dotnet run --project tests/Contador.Checks/Contador.Checks.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'No pasaron las comprobaciones del motor.' }
& dotnet run --project tests/Contador.DesktopChecks/Contador.DesktopChecks.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'No pasaron las comprobaciones de cierre y selección de cuentas.' }
& dotnet run --project tests/Contador.DesktopChecks/Contador.DesktopChecks.csproj -c Release -- --map-ui-only
if ($LASTEXITCODE -ne 0) { throw 'No pasaron las comprobaciones de la interfaz del mapa.' }
& dotnet run --project tests/Contador.DesktopChecks/Contador.DesktopChecks.csproj -c Release -- --instance-only
if ($LASTEXITCODE -ne 0) { throw 'No pasaron las comprobaciones de instancia única.' }
& dotnet run --project tests/Contador.DesktopChecks/Contador.DesktopChecks.csproj -c Release -- --tray-only
if ($LASTEXITCODE -ne 0) { throw 'No pasaron las comprobaciones de segundo plano y bandeja.' }
& dotnet run --project tests/Contador.DesktopChecks/Contador.DesktopChecks.csproj -c Release -- --team-only
if ($LASTEXITCODE -ne 0) { throw 'No pasaron las comprobaciones de equipo.' }
[xml]$project = Get-Content -LiteralPath 'src/Contador/Contador.csproj' -Raw
$version = [string]$project.Project.PropertyGroup.Version
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Version de salida no valida.' }
$distRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'dist'))
$publishDir = [IO.Path]::GetFullPath((Join-Path $distRoot $version))
if (-not $publishDir.StartsWith($distRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'La salida debe estar dentro de dist.'
}
$stagingDir = Join-Path $PSScriptRoot 'src/Contador/obj/publish-app'
& dotnet publish src/Contador/Contador.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $stagingDir
if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar Contador.' }
$executable = Join-Path $stagingDir 'Contador.exe'
if ($SigningCertificateThumbprint) {
    if (-not $TimestampServer) { throw 'Indica -TimestampServer con la URL proporcionada por tu entidad de firma.' }
    & "$PSScriptRoot/tools/Sign-Release.ps1" -Executable $executable -CertificateThumbprint $SigningCertificateThumbprint -TimestampServer $TimestampServer
} else {
    Write-Warning 'Ejecutable sin firma digital: Windows puede mostrar editor desconocido y SmartScreen.'
}
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null
Copy-Item -LiteralPath $executable -Destination (Join-Path $publishDir 'Contador.exe') -Force -ErrorAction Stop
Get-ChildItem -LiteralPath $publishDir -File | Where-Object Name -ne 'Contador.exe' | ForEach-Object {
    Remove-Item -LiteralPath $_.FullName -ErrorAction Stop
}
Write-Host "Aplicacion disponible en $publishDir\Contador.exe"

