# 17 — Seguridad HTTP, documentación e índices

## CORS

La política se configura en `Infrastructure/CorsConfig.cs`. Los orígenes
configurados en `Cors:AllowedOrigins` se combinan con una lista predeterminada
que actualmente incluye puertos locales y se añade en todos los entornos. Por
tanto, aunque `appsettings.Production.json` tenga una lista vacía, esa lista no
elimina los orígenes predeterminados. Antes de desplegar, revisa/corrige este
comportamiento y configura los orígenes reales, por ejemplo:

```text
Cors__AllowedOrigins__0=https://tienda.ejemplo.com
```

No uses `*` como sustituto de una lista de orígenes. SignalR requiere credenciales
de conexión; revisa la política antes de cambiar CORS. Los defaults actuales
están en `CorsConfig.cs`; los del perfil Development también están en
`appsettings.Development.json`.

## Límite de peticiones

`Program.cs` configura un límite global por dirección IP, por defecto 120
peticiones por 60 segundos. Se puede ajustar con
`RateLimiting__PermitLimit` y `RateLimiting__WindowSeconds`. Una petición
rechazada recibe HTTP 429 y `Retry-After`.

Si la API está detrás de un proxy inverso, no confíes automáticamente en
cabeceras de IP reenviadas: configura únicamente proxies conocidos antes de
utilizar la IP del cliente para limitar.

## Cabeceras de seguridad

Fuera de Development, el middleware de `Program.cs` añade CSP,
`X-Content-Type-Options`, `X-Frame-Options` y `Referrer-Policy`. La CSP actual
restringe recursos a `'self'`; si se sirve documentación visual en otro entorno,
revisa la política sin debilitarla innecesariamente.

## Swagger y GraphQL

- Swagger UI: `/swagger`, solo en Development.
- Documento OpenAPI: `/openapi/v1.json`, solo en Development.
- GraphQL HTTP: `/graphql`, mapeado en ambos entornos.
- Explorador visual GraphQL: habilitado únicamente en Development.
- GraphQL subscriptions: WebSocket en `/graphql`.
- SignalR: `/hubs/funkos`, requiere autenticación.

## Índices y consultas

`Infrastructure/PostgresSchemaInitializer.cs` crea la extensión `pg_trgm` e
índices para IDs relacionados, precio, stock, nombre, fecha y búsquedas por
trigramas sobre nombres activos. La base debe permitir crear la extensión y los
índices en una inicialización nueva. Los índices parciales excluyen registros
eliminados.

Antes de añadir índices, comprueba los filtros/ordenaciones existentes y el plan
de ejecución de consultas con datos representativos. Evita índices redundantes
y cambios de esquema no compatibles con instalaciones que ya tienen tablas.

## Estado de capacidades solicitadas

- CORS, rate limiting, cabeceras CSP/seguridad, Swagger, GraphQL y SignalR:
  configurados en el código actual.
- Redis: caché de aplicación; no hay Output Cache HTTP configurado.
- Envío de emails: el registro intenta enviar un correo de bienvenida. SMTP se
  configura con `Smtp__Host`, `Smtp__Port`, `Smtp__Security`, `Smtp__From`,
  `Smtp__Username` y `Smtp__Password`. Mailpit se usa en Development.
- Producción: el cleaner de inicio elimina datos; revisa
  [Docker](19-docker.md) antes de desplegar.
