# 📄 doc/20 — Testing con Bruno

---

## 1. Estructura de la colección

```
Bruno/
├── 0 - Setup/
│   └── 01 - Crear proveedor.bru
├── 1 - Categorias/
│   ├── 01 - Listar categorias.bru
│   ├── 02 - Crear categoria.bru
│   ├── 03 - Eliminar categoria con funkos (409).bru
│   ├── 04 - Eliminar categoria creada.bru
│   ├── 05 - Crear categoria invalida (400).bru
│   ├── 06 - Actualizar categoria inexistente (404).bru
│   ├── 07 - Obtener categoria por id.bru
│   ├── 08 - Actualizar categoria.bru
│   ├── 09 - Obtener categoria inexistente (404).bru
│   ├── 10 - Actualizar categoria invalida (400).bru
│   └── 11 - Eliminar categoria inexistente (404).bru
├── 2 - Funkos/
│   ├── 01 - Listar funkos.bru
│   ├── 02 - Obtener funko.bru
│   ├── 03 - Crear funko.bru
│   ├── 04 - Actualizar funko.bru
│   ├── 05 - Eliminar funko.bru
│   ├── 06 - Obtener funko inexistente (404).bru
│   ├── 07 - Buscar funkos por proveedor (filtro cruzado BD).bru
│   ├── 08 - Buscar funkos por categoria y precio.bru
│   ├── 09 - Upload imagen.bru
│   ├── 10 - Actualizar imagen.bru
│   ├── 11 - Crear funko invalido (400).bru
│   ├── 12 - Actualizar funko inexistente (404).bru
│   └── 13 - Eliminar funko inexistente (404).bru
├── 3 - Proveedores/
│   ├── 01 - Listar proveedores.bru
│   ├── 02 - Obtener proveedor por id.bru
│   ├── 03 - Actualizar proveedor.bru
│   ├── 04 - Crear proveedor invalido (400).bru
│   └── 05 - Obtener proveedor inexistente (404).bru
├── 90 - Teardown/
│   └── 01 - Eliminar proveedor.bru
├── environments/
│   └── Local.bru
├── bruno.json
└── README.md
```

---

## 2. Ejecutar con script de PowerShell

```powershell
# Verificar que Bruno CLI esté instalado
bru --version

# Ejecutar colección con entorno Local
# Requiere FUNKO_ADMIN_EMAIL y FUNKO_ADMIN_PASSWORD con credenciales de ADMIN
.\Run-Bruno.ps1
```

O manualmente:

```powershell
Push-Location .\Bruno
bru run -r --env-file .\environments\Local.bru
Pop-Location
```

---

## 3. Script `Run-Bruno.ps1`

```powershell
param(
    [string]$Environment = "Local",
    [string]$CollectionPath = ".\Bruno",
    [string]$BruPath = "bru"
)

Write-Host "🚀 Ejecutando colección Bruno..." -ForegroundColor Cyan

if (!(Get-Command $BruPath -ErrorAction SilentlyContinue)) {
    Write-Error "❌ Bruno CLI no está instalado. Instala desde https://www.usebruno.com/"
    exit 1
}

$envPath = Join-Path $CollectionPath "environments\$Environment.bru"
if (!(Test-Path $envPath)) {
    Write-Error "❌ No se encontró el entorno: $envPath"
    exit 1
}

Write-Host "📁 Colección: $CollectionPath" -ForegroundColor Yellow
Write-Host "🌍 Entorno: $Environment" -ForegroundColor Yellow

Push-Location $CollectionPath
try {
    & $BruPath run -r --env-file $envPath
} finally {
    Pop-Location
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Todos los tests de Bruno pasaron" -ForegroundColor Green
} else {
    Write-Error "❌ Algunos tests de Bruno fallaron"
    exit $LASTEXITCODE
}
```

---

## 4. Variables de entorno (`Local.bru`)

```bru
vars {
  baseUrl: http://localhost:5197
  funkoId: 1
  categoriaId: Anime000001
  proveedorId: 672f1a8b3c9d4e5f6a7b8c9d
  proveedorIdCreado: 672f1a8b3c9d4e5f6a7b8c9d
  funkoIdCreado: 1
  categoriaIdCreada: Anime000001
  categoriaIdActualizada: Anime000001
}
```

Las credenciales de ADMIN se leen de las variables de proceso `FUNKO_ADMIN_EMAIL` y `FUNKO_ADMIN_PASSWORD`; no las escribas en `Local.bru`.

---

## 5. Flujo recomendado

1. **Setup**: Crear proveedor (captura `proveedorIdCreado`)
2. **Funkos**: Listar → Obtener → Crear (captura `funkoIdCreado`) → Actualizar → Eliminar
3. **Categorías**: Listar → Crear (captura `categoriaIdCreada`) → Eliminar con funkos (409) → Eliminar creada
4. **Proveedores**: Listar → Obtener → Actualizar
5. **Teardown**: Eliminar proveedor

---

## 6. Casos de prueba por endpoint

| Endpoint | Casos probados |
|----------|---------------|
| GET /api/funkos | 200, items array, paginación, filtros |
| GET /api/funkos/{id} | 200, 404 inexistente |
| POST /api/funkos | 201, 400 inválido |
| PUT /api/funkos/{id} | 200, 404 inexistente |
| DELETE /api/funkos/{id} | 204, 404 inexistente |
| POST /api/funkos/imagen | 200, ruta relativa |
| GET /api/categorias | 200, paginación |
| GET /api/categorias/{id} | 200, 404 inexistente |
| POST /api/categorias | 201, 400 inválido, id 11 chars |
| DELETE /api/categorias/{id} | 204, 409 funkos asociados, 404 inexistente |
| GET /api/Proveedor/{id} | 200, 404 inexistente (MongoDB) |
| PUT /api/Proveedor/{id} | 200 (MongoDB) |