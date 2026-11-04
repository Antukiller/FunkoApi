# Colección Bruno - FunkoApi

Colección de peticiones para probar la API de Funko y Categoría.

## Requisitos
- API arrancada en `http://localhost:5197` (perfil `http` de `launchSettings.json`).
- Postgres, Mongo y Redis levantados (ver `compose.yaml`).

## Ejecutar
- **App Bruno**: abrir la carpeta `FunkoApiV.2/Bruno` y pulsar "Run Collection" con el entorno `Local`.
- **CLI**: `bruno run FunkoApiV.2/Bruno --env Local`

## Flujo recomendado
1. `Funkos/01 - Listar funkos` (smoke)
2. `Funkos/03 - Crear funko` (captura `funkoIdCreado`)
3. `Funkos/04 - Actualizar funko` (usa `funkoIdCreado`)
4. `Funkos/05 - Eliminar funko` (usa `funkoIdCreado`)
5. `Categorias/02 - Crear categoria` (captura `categoriaIdCreada`)
6. `Categorias/04 - Eliminar categoria creada`
7. `Categorias/03 - Eliminar categoria con funkos (409)` (debe dar 409)

Los tests (assertions) validan status codes, cuerpos y reglas de negocio.

## Nota sobre re-ejecuciones
Durante la ejecución, Bruno actualiza `environments/Local.bru` con los ids
creados (proveedorIdCreado, funkoIdCreado, categoriaIdCreada) mediante
`bru.setEnvVar`. Para volver a un estado limpio entre ejecuciones:

```powershell
git checkout FunkoApiV.2/Bruno/environments/Local.bru
