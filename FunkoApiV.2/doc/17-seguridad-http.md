# Seguridad HTTP y documentación

## CORS

Los orígenes permitidos se configuran en `Cors:AllowedOrigins`. En desarrollo se
permiten `localhost:3000` y `localhost:5173`. En producción la lista base está
vacía; configura los orígenes reales mediante variables de entorno, por ejemplo:

```text
Cors__AllowedOrigins__0=https://tienda.ejemplo.com
```

No uses `*` en producción. Si no se configura ningún origen, las llamadas sin
CORS (por ejemplo, de servidor a servidor) siguen funcionando, pero el navegador
no permitirá llamadas cross-origin.

## Límite de peticiones

Se aplica un límite fijo global por IP: 120 peticiones por cada 60 segundos.
Se puede ajustar con `RateLimiting__PermitLimit` y
`RateLimiting__WindowSeconds`. Si la API está detrás de un proxy, configura
`ForwardedHeaders` solo para proxies de confianza antes de depender de la IP del
cliente; de lo contrario, el límite puede agrupar a todos los clientes bajo la IP
del proxy.

## Headers de seguridad

En producción se envían CSP (`default-src 'self'`, sin objetos ni marcos),
`X-Content-Type-Options`, `X-Frame-Options` y `Referrer-Policy`. Swagger y la
interfaz visual de GraphQL solo se habilitan en Development.

## Swagger y GraphQL

- Swagger UI: `/swagger`, solo en Development.
- Documento OpenAPI: `/openapi/v1.json`, solo en Development.
- GraphQL sigue disponible en `/graphql` en ambos entornos; su interfaz visual
  se deshabilita en Production. El endpoint WebSocket para suscripciones sigue
  mapeado.

## Índices de PostgreSQL

El inicializador de esquema crea índices para los filtros/ordenaciones de
Funkos y activa `pg_trgm` para acelerar búsquedas `ILIKE '%texto%'`. El usuario
de base de datos que inicia la aplicación debe poder crear esa extensión e
índices; en producción es preferible aprovisionar `pg_trgm` con permisos de
administración y ejecutar cambios de esquema mediante migraciones controladas.