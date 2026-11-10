$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Instala el SDK de .NET 10 para Windows x64 desde https://dotnet.microsoft.com/download/dotnet/10.0' }
$sdkVersions = & dotnet --list-sdks
if (-not ($sdkVersions -match '^10\.')) { throw 'Se necesita el SDK de .NET 10. El runtime de .NET no basta para compilar.' }
& dotnet run --project tests/Contador.Checks/Contador.Checks.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'No pasaron las comprobaciones del motor.' }
& dotnet run --project tests/Contador.DesktopChecks/Contador.DesktopChecks.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'No pasaron las comprobaciones de cierre de la ventana.' }
[xml]$project = Get-Content -LiteralPath 'src/Contador/Contador.csproj' -Raw
$publishDir = Join-Path 'dist' ([string]$project.Project.PropertyGroup.Version)
& dotnet publish src/Contador/Contador.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $publishDir
if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar Contador.' }
Write-Host "Aplicacion disponible en $publishDir\Contador.exe"
