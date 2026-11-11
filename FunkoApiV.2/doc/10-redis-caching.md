# 10 — Redis y caché de aplicación

## Uso actual

Redis está registrado como almacén de caché distribuida y `ICacheService` se
registra como singleton. Los decoradores de `IFunkoService`,
`ICategoriaService` e `IProveedorService` gestionan las claves y la invalidación.

- Funkos: ficha por ID; los listados filtrados/paginados no se cachean.
- Categorías: ficha por ID; los listados no se cachean.
- Proveedores: ficha, listado y datos eliminados tienen claves diferenciadas.
- Las escrituras eliminan las claves afectadas.

Las claves concretas y el comportamiento están en
`Service/Funko/CachedFunkoService.cs`,
`Service/Categoria/CachedCategoriaService.cs`,
`Service/Proveedor/CachedProveedorService.cs` y la implementación de
`ICacheService`.

## Caché no es Output Cache

Redis aquí se usa para cachear datos de aplicación. El proyecto no registra
Output Cache HTTP (`AddOutputCache` / `UseOutputCache`); no documentes ni esperes
cabeceras o políticas de Output Cache.

## Reinicio y borrado

Redis usa `localhost:6379` en Development y el host `redis` en Compose. El
cleaner Production elimina las claves con prefijos `funko:*`, `categoria:*` y
`proveedor:*` en cada arranque. No implica vaciar todas las claves Redis del
servidor.
