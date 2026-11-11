# 00 — Guía práctica: qué hacer en cada momento

Esta es la guía operativa principal del repositorio. Los comandos están escritos
para PowerShell en Windows y se ejecutan desde la raíz del proyecto, donde están
`Start-Development.ps1` y `Test-All.ps1`.

## 1. Primera vez en este equipo

Necesitas:

- SDK de .NET 10.
- Docker Desktop iniciado.
- Bruno CLI solo si vas a lanzar la colección Bruno (`bru --version`).

Comprueba Docker y prepara el entorno local:

```powershell
docker info
.\Start-Development.ps1
```

El script crea `FunkoApiV.2\.env` copiando `.env.example` si aún no existe,
inicia PostgreSQL, MongoDB, Cassandra, Redis y Mailpit, y después arranca la API con
`ASPNETCORE_ENVIRONMENT=Development` en el puerto `5798`. La API queda en primer
plano en esa terminal. No cierres la terminal mientras la uses; para detenerla,
pulsa `Ctrl+C`.

El `.env` copiado contiene valores locales de ejemplo, no secretos de producción.
No subas un `.env` real al repositorio.
Los correos de prueba aparecen en Mailpit: <http://localhost:8025>.

## 2. Cada vez que quieras trabajar con la API

Desde la raíz, ejecuta:

```powershell
.\Start-Development.ps1
```

Al aparecer `Now listening on: http://localhost:5798`, abre la herramienta que
necesites:

| Para... | Dirección |
|---|---|
| Swagger / REST | <http://localhost:5798/swagger> |
| Explorador visual de GraphQL | <http://localhost:5798/graphql> |
| Consultar funkos de forma directa | <http://localhost:5798/api/funkos?page=0&size=10> |
| Hub SignalR autenticado | `http://localhost:5798/hubs/funkos` |

El endpoint SignalR es para un cliente SignalR, no para abrirlo como una página
normal. Swagger y la interfaz visual de GraphQL se habilitan en Development.
Para ejemplos de operaciones, consulta [GraphQL](21-GRAPHQL-EJEMPLOS.md).

Si prefieres arrancar desde el IDE, el perfil `http` de
`FunkoApiV.2/Properties/launchSettings.json` usa Development y el puerto 5798.
En ese caso, inicia los almacenes primero con el script; si ya están iniciados,
Docker Compose los conserva activos.

### Cambiar el puerto

```powershell
.\Start-Development.ps1 -Port 5800
```

Usa la misma URL para Swagger, GraphQL y Bruno. Para Bruno:

```powershell
.\Test-All.ps1 -BaseUrl http://localhost:5800
```

## 3. Cuentas de desarrollo

En Development, `IdentitySeeder.cs` crea estas cuentas de ejemplo si todavía no
existen:

| Rol | Email | Contraseña inicial |
|---|---|---|
| ADMIN | `admin@admin.com` | `Admin123!` |
| USER | `user@user.com` | `User123!` |

Son únicamente credenciales locales para pruebas; nunca las uses en producción.
Si cambias la contraseña de una cuenta ya creada, el seeder no la restablece.
Para obtener un JWT, envía `POST /api/auth/login` con email y contraseña; el
token se utiliza como `Authorization: Bearer <token>` en operaciones protegidas.

## 4. Probar GraphQL

1. Arranca la API en Development.
2. Abre <http://localhost:5798/graphql>.
3. Ejecuta una consulta de lectura de los ejemplos de
   [GraphQL](21-GRAPHQL-EJEMPLOS.md).
4. Para una mutación, inicia sesión y añade al panel **Headers**:
   `Authorization: Bearer <JWT>`.

Las mutaciones requieren rol `ADMIN`. El explorador visual no está habilitado
en Production, aunque el endpoint GraphQL sigue mapeado. Las suscripciones usan
WebSocket en `/graphql` y SignalR usa `/hubs/funkos`; son protocolos distintos.

## 5. Ejecutar pruebas

### Suite completa

Deja Docker Desktop iniciado y la API accesible en Development. Desde la raíz:

```powershell
.\Test-All.ps1
```

El script ejecuta primero `dotnet test` (incluidas las pruebas de integración
con Testcontainers) y después toda la colección Bruno. Si las credenciales ADMIN
no están definidas, las solicita sin guardarlas en el repositorio. Bruno CLI
debe estar instalado.

### Solo pruebas .NET

```powershell
dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj
```

Las pruebas de integración crean sus propios contenedores y necesitan Docker.
Para ejecutar solo unitarias:

```powershell
dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj --filter "FullyQualifiedName~Unit"
```

### Solo Bruno

Define las variables en la sesión actual, ejecuta el script y bórralas al
terminar. Sustituye la contraseña si la cuenta local tiene otra:

```powershell
$env:FUNKO_ADMIN_EMAIL = "admin@admin.com"
$env:FUNKO_ADMIN_PASSWORD = "Admin123!"
.\Run-Bruno.ps1
Remove-Item Env:FUNKO_ADMIN_EMAIL, Env:FUNKO_ADMIN_PASSWORD
```

`Test-All.ps1` pide estas credenciales de forma segura si no las encuentra.
No pongas contraseñas en `Bruno/environments/Local.bru`, en documentación
compartida ni en comandos que vayan a guardarse en el historial.

## 6. Entornos y comportamiento de los datos

| Entorno | Al arrancar |
|---|---|
| Development | Aplica el esquema y carga datos de ejemplo en PostgreSQL, MongoDB, Cassandra e Identity. |
| Production | Borra los datos de aplicación de PostgreSQL, los proveedores MongoDB, las tablas Cassandra y las claves Redis con prefijos `funko:*`, `categoria:*` y `proveedor:*`. No crea cuentas de ejemplo; garantiza los roles `ADMIN` y `USER`. |

El borrado de Production ocurre en **cada arranque del proceso backend**, aunque
se conserven los volúmenes Docker. El registro HTTP crea usuarios con rol
`USER`; no crea un administrador. No despliegues ni reinicies el backend en
Production si necesitas conservar esos datos. Además, revisa la política CORS
antes de exponerlo: sus orígenes predeterminados locales se añaden incluso en
Production (detalle en [Seguridad HTTP](17-seguridad-http.md)).

## 7. Errores habituales

### `compose.yaml` no encontrado

Ejecuta los scripts desde la raíz del repositorio. El Compose está en
`FunkoApiV.2\compose.yaml`; no añadas dos veces `FunkoApiV.2` a la ruta.

### `POSTGRES_PASSWORD` o `JWT_KEY` no definidos

`Start-Development.ps1` crea `FunkoApiV.2\.env` desde la plantilla si falta.
Comprueba que el fichero exista y que lo estás arrancando con el script. Docker
Compose valida variables de todos los servicios declarados, incluido `backend`,
aunque solo se arranquen los almacenes.

### El puerto 5798 ya está ocupado

Identifica el proceso que escucha en el puerto:

```powershell
Get-NetTCPConnection -LocalPort 5798 -State Listen |
  Select-Object LocalAddress, LocalPort, OwningProcess
Get-Process -Id <PID>
```

Si es una instancia tuya de la API, deténla con `Ctrl+C` en su terminal. Si no
puedes usar ese puerto, arranca en otro con `-Port`.

### Error MSB3021/MSB3027: DLL bloqueada por `FunkoApiV.2`

La API continúa ejecutándose y Windows impide reemplazar la DLL que está usando.
Detén la instancia con `Ctrl+C` y vuelve a compilar o ejecutar los tests. Si no
localizas la terminal, identifica el PID con `Get-Process -Id <PID>` antes de
detenerlo; no finalices procesos que no hayas confirmado que pertenecen a tu API.

### Swagger muestra error al cargar operaciones

Confirma que abriste la URL de la instancia actual y que estás en Development.
La definición OpenAPI se genera en `/swagger/v1/swagger.json`; si esa URL da
error, revisa la excepción y los logs de la API.

## 8. Apagar servicios

`Ctrl+C` detiene la API. Para detener únicamente los almacenes de desarrollo:

```powershell
docker compose --env-file .\FunkoApiV.2\.env `
  -f .\FunkoApiV.2\compose.yaml stop db mongo_db cassandra redis
```

Para instrucciones del stack completo y las precauciones de Production, consulta
[Docker](19-docker.md).

## Mapa de apuntes técnicos

| Documento | Tema |
|---|---|
| [01](01-configuracion-proyectos-dotnet.md) | Solución, proyectos .NET y configuración |
| [02](02-arquitectura-pipeline-http.md) | Arquitectura y pipeline HTTP |
| [03](03-inyeccion-dependencias.md) | Inyección de dependencias |
| [04](04-controladores-rest.md) | Controladores REST y respuestas |
| [05](05-validacion-cascada.md) | Validación de solicitudes |
| [06](06-rest-best-practices.md) | Contrato y buenas prácticas REST |
| [07](07-repository-pattern.md) | Repositorios |
| [08](08-ef-core-postgresql.md) | EF Core y PostgreSQL |
| [09](09-mongodb.md) | MongoDB y proveedores |
| [10](10-redis-caching.md) | Redis y caché de aplicación |
| [11](11-patron-result.md) | Result y errores de dominio |
| [12](12-servicios-negocio.md) | Servicios de negocio |
| [13](13-borrado-logico.md) | Borrado lógico y limpieza Production |
| [14](14-mapeadores.md) | Mapeadores |
| [15](15-jwt-authentication.md) | Autenticación JWT |
| [16](16-autorizacion-roles.md) | Autorización por roles |
| [17](17-seguridad-http.md) | CORS, límites, CSP, Swagger e índices |

Las páginas técnicas describen el estado actual y enlazan con las clases
correspondientes. Para ejecutar la aplicación o las pruebas, empieza por las
secciones anteriores de esta guía.
