[CmdletBinding()]
param(
    [ValidateRange(1, 65535)]
    [int]$Port = 5798,
    [switch]$ResetData
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

if (!(Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "No se encontró Docker. Instala Docker Desktop y vuelve a ejecutar el script."
}

try {
    & docker info --format '{{.ServerVersion}}' 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker no está disponible."
    }
} catch {
    throw "Docker no está disponible. Inicia Docker Desktop y vuelve a ejecutar el script."
}

if (!(Test-Path -LiteralPath $envFile)) {
    if (!(Test-Path -LiteralPath $exampleEnvFile)) {
        throw "No se encontró $envFile ni la plantilla $exampleEnvFile."
    }

    Copy-Item -LiteralPath $exampleEnvFile -Destination $envFile
    Write-Host "Se creó .env con los valores de desarrollo de .env.example." -ForegroundColor Yellow
}

if ($ResetData) {
    $confirm = Read-Host "Esto borrará los volúmenes de desarrollo (Postgres, Mongo, Cassandra y Redis). Escribe SI para continuar"
    if ($confirm -ne "SI") {
        throw "Cancelado por el usuario."
    }

    Write-Host "==> Borrando volúmenes de desarrollo..." -ForegroundColor Yellow
    & docker compose --env-file $envFile -f $composeFile down -v
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose no pudo borrar los volúmenes (código $LASTEXITCODE)."
    }
}

$portInUse = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
if ($portInUse) {
    throw "El puerto $Port ya está ocupado. Detén la aplicación que lo usa o ejecuta .\Start-Development.ps1 -Port 5800."
}

Write-Host "==> Iniciando PostgreSQL, MongoDB, Cassandra, Redis y Mailpit..." -ForegroundColor Cyan
& docker compose --profile development --env-file $envFile -f $composeFile up -d --wait db mongo_db cassandra redis mailpit
if ($LASTEXITCODE -ne 0) {
    throw "Docker Compose no pudo iniciar los servicios (código $LASTEXITCODE)."
}

# POSTGRES_PASSWORD solo se aplica al crear el volumen. Si el volumen ya existía
# con otra contraseña, la API fallará con 28P01. Lo comprobamos antes de arrancar.
function Get-DotEnvValue {
    param(
        [string]$Path,
        [string]$Key
    )

    $match = Select-String -Path $Path -Pattern "^$([regex]::Escape($Key))=(.*)$" |
        Select-Object -First 1
    if ($null -eq $match) {
        return $null
    }

    return $match.Matches[0].Groups[1].Value.Trim()
}

$postgresUser = Get-DotEnvValue -Path $envFile -Key "POSTGRES_USER"
if ([string]::IsNullOrWhiteSpace($postgresUser)) {
    $postgresUser = "postgres"
}
$postgresPassword = Get-DotEnvValue -Path $envFile -Key "POSTGRES_PASSWORD"
if ([string]::IsNullOrWhiteSpace($postgresPassword)) {
    throw "Falta POSTGRES_PASSWORD en $envFile."
}

Write-Host "==> Comprobando credenciales de PostgreSQL..." -ForegroundColor Cyan
$postgresNetwork = (& docker inspect funkoapi_postgres --format '{{range $k, $v := .NetworkSettings.Networks}}{{$k}}{{end}}' 2>$null)
if ([string]::IsNullOrWhiteSpace($postgresNetwork)) {
    throw "No se pudo inspeccionar el contenedor funkoapi_postgres. ¿Está arrancado?"
}

& docker run --rm --network $postgresNetwork `
    -e PGPASSWORD=$postgresPassword `
    postgres:16-alpine `
    psql -h db -U $postgresUser -d postgres -tAc "select 1" 2>$null | Out-Null

if ($LASTEXITCODE -ne 0) {
    $alterUser = 'docker exec -it funkoapi_postgres psql -U postgres -c "ALTER USER postgres WITH PASSWORD ''' + $postgresPassword + ''';"'
    Write-Host ""
    Write-Host "POSTGRES_PASSWORD no coincide con la contraseña guardada en el volumen." -ForegroundColor Red
    Write-Host "POSTGRES_PASSWORD solo se aplica cuando el volumen está vacío." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Para conservar los datos, cambia la contraseña dentro de Postgres:" -ForegroundColor Yellow
    Write-Host "  $alterUser"
    Write-Host "O borra los volúmenes de desarrollo (se perderán los datos): .\Start.ps1 -ResetData" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Recuerda que .env y appsettings.Development.json deben usar la misma contraseña." -ForegroundColor Yellow
    throw "Credenciales de PostgreSQL incorrectas. La API no se ha arrancado."
}

Write-Host "    Credenciales correctas." -ForegroundColor Green

Write-Host ""
Write-Host "Servicios listos:" -ForegroundColor Green
Write-Host "  Swagger:        http://localhost:$Port/swagger"
Write-Host "  GraphQL:        http://localhost:$Port/graphql"
Write-Host "  Demo SignalR:   http://localhost:$Port/"
Write-Host "  Mailpit:        http://localhost:8025"
Write-Host "  Admin:          admin@admin.com / Admin123!"
Write-Host ""

$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
try {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    Push-Location $apiDirectory
    try {
        Write-Host "==> Iniciando la API en desarrollo en http://localhost:$Port" -ForegroundColor Cyan
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
