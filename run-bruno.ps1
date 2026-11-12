#!/usr/bin/env pwsh
# run-bruno.ps1
# Delegación desde la raíz del repositorio a FunkoApiV.2\Run-Bruno.ps1.
# Uso: .\run-bruno.ps1 [-Environment Local] [-ResetData]

& (Join-Path $PSScriptRoot "FunkoApiV.2\Run-Bruno.ps1") @args
exit $LASTEXITCODE
