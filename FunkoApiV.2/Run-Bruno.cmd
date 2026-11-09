@echo off
::===============================================================================
:: Run-Bruno.cmd
:: Ejecuta la colección Bruno de FunkoApi con el entorno Local.
::===============================================================================

@echo.
@echo ==============================================
@echo 🚀 FunkoApi - Ejecutando colección Bruno
@echo ==============================================
@echo.

:: Ir a la carpeta raíz del proyecto
cd /d "%~dp0"

:: Verificar si Bruno CLI está instalado
where bru >nul 2>&1
if %errorlevel% neq 0 (
    echo ⚠️  Bruno CLI no está instalado en el PATH.
    echo.
    echo Opciones para instalar Bruno CLI:
    echo   1. winget install --id Usabruno.bruno
    echo   2. choco install bruno -y
    echo   3. Manual: https://www.usebruno.com/cli
    echo.
    echo Mientras tanto, usa: Test-Api.cmd
    echo.
    pause
    exit /b 1
)

echo ✅ Bruno CLI encontrado.

:: Verificar que la colección exista
if not exist "Bruno\environments\Local.bru" (
    echo ❌ No se encontró la colección Bruno o el entorno Local.bru
    echo.
    pause
    exit /b 1
)

echo 📁 Colección: .\Bruno
echo 🌍 Entorno: Local
echo.

if not defined FUNKO_ADMIN_EMAIL (
    echo ❌ Define FUNKO_ADMIN_EMAIL con el email de una cuenta ADMIN.
    exit /b 1
)
if not defined FUNKO_ADMIN_PASSWORD (
    echo ❌ Define FUNKO_ADMIN_PASSWORD con la contraseña de una cuenta ADMIN.
    exit /b 1
)

:: Verificar que la API esté corriendo en el puerto del entorno
echo 🔍 Verificando que la API esté disponible en http://localhost:5197...
powershell -ExecutionPolicy Bypass -Command "try { \$r = Invoke-WebRequest -Uri 'http://localhost:5197/api/funkos?page=0&size=1' -ErrorAction Stop; Write-Output \$r.StatusCode } catch { Write-Output 'ERROR' }" 2>nul | findstr "200" >nul 2>&1
if %errorlevel% neq 0 (
    echo ⚠️  La API no parece estar corriendo en http://localhost:5197
    echo   Ejecuta primero: dotnet run --project FunkoApiV.2\FunkoApiV.2.csproj
    echo.
    echo ¿Continuar de todas formas? (s/n):
    set /p confirm=
    if /i not "%confirm%"=="s" (
        exit /b 1
    )
    echo.
)

:: Ejecutar Bruno con el entorno Local
echo ▶️  Ejecutando tests de Bruno...
pushd Bruno
bru run -r --env-file .\environments\Local.bru
set "brunoExitCode=%errorlevel%"
popd

if "%brunoExitCode%"=="0" (
    echo.
    echo ==============================================
    echo ✅ Todos los tests de Bruno pasaron correctamente
    echo ==============================================
) else (
    echo.
    echo ==============================================
    echo ❌ Algunos tests de Bruno fallaron
    echo ==============================================
)

pause
exit /b %brunoExitCode%
