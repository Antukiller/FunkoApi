[CmdletBinding()]
param(
    [ValidateRange(1, 65535)]
    [int]$Port = 5798
)

$ErrorActionPreference = "Stop"

$projectRoot = $PSScriptRoot
$apiDirectory = Join-Path $projectRoot "FunkoApiV.2"
$composeFile = Join-Path $apiDirectory "compose.yaml"
$envFile = Join-Path $apiDirectory ".env"
$exampleEnvFile = Join-Path $projectRoot ".env.example"

if (!(Test-Path -LiteralPath $composeFile)) {
    throw "No se encontró Docker Compose: $composeFile"
}

if (!(Test-Path -LiteralPath $envFile)) {
    if (!(Test-Path -LiteralPath $exampleEnvFile)) {
        throw "No se encontró $envFile ni la plantilla $exampleEnvFile."
    }

    Copy-Item -LiteralPath $exampleEnvFile -Destination $envFile
    Write-Host "Se creó .env con los valores de desarrollo de .env.example."
}

$portInUse = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
if ($portInUse) {
    throw "El puerto $Port ya está ocupado. Detén la aplicación que lo usa o ejecuta .\Start-Development.ps1 -Port 5197."
}

Write-Host "Iniciando PostgreSQL, MongoDB, Cassandra, Redis y Mailpit..."
& docker compose --profile development --env-file $envFile -f $composeFile up -d db mongo_db cassandra redis mailpit
if ($LASTEXITCODE -ne 0) {
    throw "Docker Compose no pudo iniciar los servicios (código $LASTEXITCODE)."
}

$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
try {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    Push-Location $apiDirectory
    try {
        Write-Host "Iniciando la API en desarrollo en http://localhost:$Port"
        & dotnet run --project .\FunkoApiV.2.csproj --no-launch-profile --urls "http://localhost:$Port"
        if ($LASTEXITCODE -ne 0) {
            throw "La API terminó con código $LASTEXITCODE."
        }
    } finally {
        Pop-Location
    }
} finally {
    if ($null -eq $previousEnvironment) {
        Remove-Item Env:ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
    } else {
        $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
    }
}
