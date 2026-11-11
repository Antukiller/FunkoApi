# 13 — Borrado lógico y reglas de eliminación

## Datos de negocio

- **Funko:** el endpoint DELETE marca el registro como eliminado; las consultas
  normales excluyen los eliminados.
- **Categoría:** se marca como eliminada solo cuando no tiene Funkos asociados.
  Si existe alguno, la API responde `409 Conflict`.
- **Proveedor:** usa operaciones de baja lógica propias del servicio MongoDB y
  expone una consulta separada para proveedores eliminados.

Los detalles de filtrado están en los repositorios y servicios. Antes de
modificar `IsDeleted`, comprueba filtros, caché y consultas cruzadas en
PostgreSQL/MongoDB/Cassandra.

## Datos de Production: limpieza física separada

El comportamiento anterior es el borrado solicitado por un cliente. Además,
`ProductionDataCleaner.ClearAsync` vacía físicamente los datos de aplicación en
cada inicio Production:

- Tablas de Funkos, categorías y usuarios de Identity en PostgreSQL.
- Colección de proveedores en MongoDB.
- Tablas de lectura de Cassandra.
- Claves Redis con prefijos `funko:*`, `categoria:*` y `proveedor:*`.

Los roles de Identity se conservan o se crean. No hay seed de usuarios
Production. Este borrado de inicio no es un borrado lógico y no puede
recuperarse con un filtro de API.

**No arranques el backend con `ASPNETCORE_ENVIRONMENT=Production` sobre datos
que quieras conservar.** Consulta [Docker](19-docker.md) antes de usar Compose.
