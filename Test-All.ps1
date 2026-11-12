#!/usr/bin/env pwsh
# Test-All.ps1
# Delegación desde la raíz del repositorio a FunkoApiV.2\Test-All.ps1.
# Uso: .\Test-All.ps1 [-BaseUrl http://localhost:5798]

& (Join-Path $PSScriptRoot "FunkoApiV.2\Test-All.ps1") @args
exit $LASTEXITCODE
