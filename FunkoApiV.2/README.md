# FunkoApi

API REST para gestionar Funkos, categorías, proveedores y usuarios. Está desarrollada con ASP.NET Core/.NET 10 y usa PostgreSQL, MongoDB, Cassandra y Redis. También ofrece GraphQL, SignalR, Swagger en desarrollo y una colección de pruebas Bruno.

## ¿Qué necesito hacer ahora?

| Si quieres... | Sigue esta guía |
|---|---|
| Preparar el equipo y arrancar la API en desarrollo | [Guía práctica](doc/00-guia-practica.md) |
| Ejecutar las pruebas .NET y Bruno | [Testing](doc/18-testing.md) y [Bruno](doc/20-bruno-testing.md) |
| Probar consultas, mutaciones o suscripciones GraphQL | [Ejemplos GraphQL](doc/21-GRAPHQL-EJEMPLOS.md) |
| Entender los servicios Docker y producción | [Docker](doc/19-docker.md) |
| Consultar las decisiones de seguridad HTTP | [Seguridad HTTP](doc/17-seguridad-http.md) |

**Arranque habitual en Windows, desde esta carpeta:**

```powershell
.\Start-Development.ps1
```

Cuando la API indique que está escuchando, abre:

- Swagger: <http://localhost:5798/swagger>
- GraphQL y su explorador visual: <http://localhost:5798/graphql>
- SignalR: <http://localhost:5798/hubs/funkos> (requiere JWT)

Para apagarla, usa `Ctrl+C` en la misma terminal.

## Qué incluye el proyecto

- REST para Funkos, categorías, proveedores y autenticación.
- GraphQL con consultas, mutaciones, cargas y suscripciones.
- SignalR en el hub autenticado `/hubs/funkos`.
- PostgreSQL para Funkos, categorías e Identity; MongoDB para proveedores; Cassandra para consultas agregadas; Redis para caché.
- Email de bienvenida al registrar usuarios; en desarrollo los mensajes se capturan con Mailpit.
- Validación con FluentValidation, respuestas de error coherentes, autorización JWT por roles y borrado lógico en Funkos/categorías.
- Límites de peticiones, CORS configurable, cabeceras CSP/seguridad en producción e índices PostgreSQL para filtros y búsquedas.
- Tests .NET y colección Bruno.

**Aún no forma parte del código:** Output Cache HTTP. Redis está integrado como caché de aplicación; no equivale a Output Cache.

## Entornos y datos: lee antes de arrancar producción

- **Development:** al iniciar la API, aplica el esquema e inserta datos de ejemplo en PostgreSQL, MongoDB, Cassandra e Identity. El seeding es idempotente para los datos existentes. Las cuentas de ejemplo solo se crean en este entorno.
- **Production:** al iniciar la API, elimina los datos de las tablas de aplicación PostgreSQL, los proveedores MongoDB, las tablas Cassandra y las claves Redis con prefijos `funko:*`, `categoria:*` y `proveedor:*`. No inserta usuarios de ejemplo; conserva o crea los roles `ADMIN` y `USER`.

Por tanto, **cada reinicio del backend en producción vuelve a vaciar esos datos**. No uses Production para guardar datos que quieras conservar. El registro público crea usuarios `USER`, no crea administradores. Revisa [la guía de Docker](doc/19-docker.md) antes de desplegar.

> El Compose actual ejecuta la API y cada almacén en servicios/contenedores separados; no es una imagen única autocontenida. Levantar el backend en Production activa el borrado anterior.

> **Antes de producción:** revisa también CORS. La configuración actual combina
> los orígenes configurados con orígenes locales predeterminados en todos los
> entornos; se explica en [Seguridad HTTP](doc/17-seguridad-http.md).

## Estructura

```text
FunkoApiV.2/
├── FunkoApiV.2/       # API, controladores, servicios, repositorios e infraestructura
├── FunkoApiV2Test/    # Tests unitarios y de integración
├── Bruno/             # Colección de pruebas HTTP
├── doc/               # Guías operativas y apuntes técnicos
├── Start-Development.ps1
├── Test-All.ps1
└── README.md
```

## Documentación

- [00 — Guía práctica: preparar, arrancar, probar y solucionar problemas](doc/00-guia-practica.md)
- [17 — Seguridad HTTP, Swagger, GraphQL e índices](doc/17-seguridad-http.md)
- [18 — Tests .NET y cobertura](doc/18-testing.md)
- [19 — Docker, desarrollo y producción](doc/19-docker.md)
- [20 — Bruno](doc/20-bruno-testing.md)
- [21 — Ejemplos GraphQL](doc/21-GRAPHQL-EJEMPLOS.md)
- [Índice de los apuntes 01–17](doc/00-guia-practica.md#mapa-de-apuntes-técnicos)

## Tecnología y versiones

Los `PackageReference` y el framework objetivo están en `FunkoApiV.2/FunkoApiV.2.csproj` y `FunkoApiV2Test/FunkoApiV2Test.csproj`. Consulta esos archivos como fuente de verdad: las versiones de NuGet evolucionan y no deben copiarse de tablas antiguas.

## Atribución

El proyecto utiliza como referencia material de José Luis González Sánchez
(`joseluisgs`) y sus repositorios docentes, citados en los apuntes
correspondientes. Respeta las licencias y atribuciones del material original si
reutilizas contenido.
