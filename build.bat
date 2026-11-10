@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1"
if errorlevel 1 (echo. & echo La compilacion no se completo. & pause & exit /b 1)
echo.
echo Listo: revisa la ruta del ejecutable indicada arriba.
pause
