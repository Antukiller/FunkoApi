@echo off
::===============================================================================
:: Test-Api.cmd
:: Prueba los endpoints de FunkoApi usando PowerShell nativo (Invoke-WebRequest).
:: No requiere instalar Bruno CLI.
::===============================================================================

@echo.
@echo ==============================================
@echo 🧪 FunkoApi - Pruebas de API (PowerShell nativo)
@echo ==============================================
@echo.

:: Ir a la carpeta raíz del proyecto
cd /d "%~dp0"

:: Verificar que la API esté corriendo
echo 🔍 Verificando que la API esté disponible en http://localhost:5000...
powershell -ExecutionPolicy Bypass -Command "try { \$r = Invoke-WebRequest -Uri 'http://localhost:5000/api/funkos?page=0&size=1' -ErrorAction Stop; Write-Output \$r.StatusCode } catch { Write-Output 'ERROR' }" 2>nul | findstr "200" >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ No se puede conectar a la API en http://localhost:5000
    echo.
    echo 📋 Para levantar la API, ejecuta en otra terminal:
    echo   dotnet run --project FunkoApiV.2\FunkoApiV.2.csproj
    echo.
    pause
    exit /b 1
)

echo ✅ API disponible.
echo.

:: Ejecutar script de pruebas
powershell -ExecutionPolicy Bypass -File .\Test-Api.ps1

pause
exit /b %errorlevel%
