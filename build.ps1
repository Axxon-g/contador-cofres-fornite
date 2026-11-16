param([string]$SigningCertificateThumbprint, [uri]$TimestampServer)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Instala el SDK de .NET 10 para Windows x64 desde https://dotnet.microsoft.com/download/dotnet/10.0' }
$sdkVersions = & dotnet --list-sdks
if (-not ($sdkVersions -match '^10\.')) { throw 'Se necesita el SDK de .NET 10. El runtime de .NET no basta para compilar.' }
& dotnet build tests/Contador.Checks/Contador.Checks.csproj -c Release --disable-build-servers -m:1 -p:UseSharedCompilation=false
if ($LASTEXITCODE -ne 0) { throw 'No se pudieron compilar las comprobaciones del motor.' }
& dotnet build tests/Contador.DesktopChecks/Contador.DesktopChecks.csproj -c Release --disable-build-servers -m:1 -p:UseSharedCompilation=false
if ($LASTEXITCODE -ne 0) { throw 'No se pudieron compilar las comprobaciones de escritorio.' }
& dotnet run --project tests/Contador.Checks/Contador.Checks.csproj -c Release --no-build --no-restore
if ($LASTEXITCODE -ne 0) { throw 'No pasaron las comprobaciones del motor.' }
$desktopChecks = @(
    @{ Arguments = @(); Label = 'cierre y selección de cuentas' },
    @{ Arguments = @('--map-ui-only'); Label = 'mapa' },
    @{ Arguments = @('--instance-only'); Label = 'instancia única' },
    @{ Arguments = @('--tray-only'); Label = 'bandeja' },
    @{ Arguments = @('--team-only'); Label = 'equipo' },
    @{ Arguments = @('--live-view'); Label = 'conteo opcional' },
    @{ Arguments = @('--shell-refresh'); Label = 'pantallas y fechas' },
    @{ Arguments = @('--stats-view'); Label = 'estadísticas públicas sin clave' }
)
foreach ($check in $desktopChecks) {
    & dotnet run --project tests/Contador.DesktopChecks/Contador.DesktopChecks.csproj -c Release --no-build --no-restore -- @($check.Arguments)
    if ($LASTEXITCODE -ne 0) { throw "No pasaron las comprobaciones de $($check.Label)." }
}
[xml]$project = Get-Content -LiteralPath 'src/Contador/Contador.csproj' -Raw
$version = [string]$project.Project.PropertyGroup.Version
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Version de salida no valida.' }
$distRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'dist'))
$publishDir = [IO.Path]::GetFullPath((Join-Path $distRoot $version))
if (-not $publishDir.StartsWith($distRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'La salida debe estar dentro de dist.'
}
$stagingDir = Join-Path $PSScriptRoot 'src/Contador/obj/publish-app'
& dotnet publish src/Contador/Contador.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $stagingDir --disable-build-servers -m:1 -p:UseSharedCompilation=false
if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar Contador.' }
$executable = Join-Path $stagingDir 'Contador.exe'
if ($SigningCertificateThumbprint) {
    if (-not $TimestampServer) { throw 'Indica -TimestampServer con la URL proporcionada por tu entidad de firma.' }
    & "$PSScriptRoot/tools/Sign-Release.ps1" -Executable $executable -CertificateThumbprint $SigningCertificateThumbprint -TimestampServer $TimestampServer
} else {
    Write-Warning 'Ejecutable sin firma digital: Windows puede mostrar editor desconocido y SmartScreen.'
}
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null
$destination = Join-Path $publishDir 'Contador.exe'
try {
    Copy-Item -LiteralPath $executable -Destination $destination -Force -ErrorAction Stop
} catch {
    throw "No se pudo reemplazar Contador.exe. Si está abierto, cierra desde la bandeja → Salir y vuelve a compilar. Detalle: $($_.Exception.Message)"
}
if ((Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash -ne
    (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash) {
    throw 'La copia de Contador.exe no coincide con la compilación.'
}
Write-Host "Aplicacion disponible en $publishDir\Contador.exe"

