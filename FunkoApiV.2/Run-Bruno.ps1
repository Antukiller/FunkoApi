#!/usr/bin/env pwsh
#===============================================================================
# Run-Bruno.ps1
# Ejecuta la colección Bruno de FunkoApi con tests automatizados.
# Si Bruno CLI no está instalado, intenta instalarlo o da instrucciones.
#===============================================================================

[CmdletBinding()]
param(
    [string]$Environment = "Local",
    [string]$CollectionPath = (Join-Path $PSScriptRoot "Bruno"),
    [string]$BrunoCli = "bru"
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

if (-not [System.IO.Path]::IsPathRooted($CollectionPath)) {
    $CollectionPath = Join-Path $PSScriptRoot $CollectionPath
}
$CollectionPath = (Resolve-Path -LiteralPath $CollectionPath -ErrorAction Stop).Path

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "🚀 FunkoApi - Ejecutando colección Bruno" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host ""

# ─── 1. Verificar si Bruno CLI está instalado ───
$brunoCmd = Get-Command $BrunoCli -ErrorAction SilentlyContinue

if (-not $brunoCmd) {
    Write-Host "⚠️  Bruno CLI no está instalado en el PATH." -ForegroundColor Yellow
    Write-Host ""

    # Intentar instalar con winget (Windows 10/11 moderno)
    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if ($winget) {
        Write-Host "📦 Intentando instalar Bruno CLI con winget..." -ForegroundColor Cyan
        try {
            & winget install --id Usabruno.bruno --accept-package-agreements --accept-source-agreements
            $installExit = $LASTEXITCODE
            if ($installExit -eq 0) {
                Write-Host "✅ Bruno CLI instalado correctamente con winget." -ForegroundColor Green
                $brunoCmd = Get-Command $BrunoCli -ErrorAction SilentlyContinue
            } else {
                Write-Host "⚠️  winget no pudo instalar Bruno (código: $installExit)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "⚠️  Error al intentar instalar con winget: $_" -ForegroundColor Yellow
        }
    }

    # Si sigue sin estar, intentar con chocolatey
    if (-not $brunoCmd) {
        $choco = Get-Command choco -ErrorAction SilentlyContinue
        if ($choco) {
            Write-Host "📦 Intentando instalar Bruno CLI con chocolatey..." -ForegroundColor Cyan
            try {
                & choco install bruno -y
                $installExit = $LASTEXITCODE
                if ($installExit -eq 0) {
                    Write-Host "✅ Bruno CLI instalado correctamente con chocolatey." -ForegroundColor Green
                    $brunoCmd = Get-Command $BrunoCli -ErrorAction SilentlyContinue
                } else {
                    Write-Host "⚠️  chocolatey no pudo instalar Bruno (código: $installExit)" -ForegroundColor Yellow
                }
            } catch {
                Write-Host "⚠️  Error al intentar instalar con chocolatey: $_" -ForegroundColor Yellow
            }
        }
    }
}

# ─── 2. Si sigue sin Bruno, dar instrucciones y salir ───
if (-not $brunoCmd) {
    Write-Host ""
    Write-Host "==============================================" -ForegroundColor Red
    Write-Host "❌ Bruno CLI no está disponible." -ForegroundColor Red
    Write-Host "==============================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Opciones para instalar Bruno CLI:" -ForegroundColor Yellow
    Write-Host "  1. winget:    winget install --id Usabruno.bruno" -ForegroundColor White
    Write-Host "  2. chocolatey: choco install bruno -y" -ForegroundColor White
    Write-Host "  3. Manual:    https://www.usebruno.com/cli" -ForegroundColor White
    Write-Host ""
    Write-Host "Después de instalar, reinicia PowerShell y vuelve a ejecutar este script." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Mientras tanto, puedes usar el script alternativo:" -ForegroundColor Cyan
    Write-Host "  .\Test-Api.ps1" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "✅ Bruno CLI encontrado: $($brunoCmd.Source)" -ForegroundColor Green

# ─── 3. Verificar colección y entorno ───
if (!(Test-Path $CollectionPath)) {
    Write-Error "❌ No se encontró la carpeta de la colección: $CollectionPath"
    exit 1
}

$envFile = Join-Path $CollectionPath "environments\$Environment.bru"
if (!(Test-Path $envFile)) {
    Write-Error "❌ No se encontró el archivo de entorno: $envFile"
    Write-Host ""
    Write-Host "Entornos disponibles:" -ForegroundColor Yellow
    $envDir = Join-Path $CollectionPath "environments"
    if (Test-Path $envDir) {
        Get-ChildItem -Path $envDir -Filter "*.bru" | ForEach-Object {
            Write-Host "   - $($_.Name)" -ForegroundColor Yellow
        }
    }
    exit 1
}

Write-Host ""
Write-Host "📁 Colección: $CollectionPath" -ForegroundColor Yellow
Write-Host "🌍 Entorno:   $Environment ($envFile)" -ForegroundColor Yellow
Write-Host ""

if ([string]::IsNullOrWhiteSpace($env:FUNKO_ADMIN_EMAIL) -or
    [string]::IsNullOrWhiteSpace($env:FUNKO_ADMIN_PASSWORD)) {
    Write-Error "Define FUNKO_ADMIN_EMAIL y FUNKO_ADMIN_PASSWORD para iniciar sesión como ADMIN."
    exit 1
}

# ─── 4. Ejecutar Bruno ───
Write-Host "▶️  Ejecutando tests de Bruno...`n" -ForegroundColor Cyan

Push-Location $CollectionPath
try {
    & $BrunoCli run -r --env-file $envFile
    $exitCode = $LASTEXITCODE
} finally {
    Pop-Location
}

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "==============================================" -ForegroundColor Green
    Write-Host "✅ Todos los tests de Bruno pasaron correctamente" -ForegroundColor Green
    Write-Host "==============================================" -ForegroundColor Green
} else {
    Write-Host "==============================================" -ForegroundColor Red
    Write-Host "❌ Algunos tests de Bruno fallaron (código: $exitCode)" -ForegroundColor Red
    Write-Host "==============================================" -ForegroundColor Red
}

exit $exitCode
