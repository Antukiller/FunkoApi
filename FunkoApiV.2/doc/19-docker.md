# 19 — Docker: desarrollo y producción

El Compose de referencia está en `FunkoApiV.2/compose.yaml`; los comandos de
esta guía se ejecutan desde la raíz del repositorio.

## 1. Servicios del Compose

| Servicio | Uso | Puerto del host |
|---|---|---:|
| `db` | PostgreSQL: Funkos, categorías e Identity | 5432 |
| `mongo_db` | MongoDB: proveedores | 27017 |
| `cassandra` | Cassandra: vistas de consulta | 9043 → 9042 |
| `redis` | Caché | 6379 |
| `mailpit` | SMTP y bandeja de correo local (perfil `development`) | 1025 / 8025 |
| `backend` | API ASP.NET Core | 5197 → 8080 |

El Compose actual contiene cinco servicios de apoyo y el backend. **No es una única imagen
con la API y las bases de datos dentro del mismo contenedor.**

## 2. Desarrollo local (recomendado)

Para desarrollar, Docker ejecuta los cuatro almacenes y `dotnet` ejecuta la API
en el host. Esto permite usar Hot Reload/debugger y no inicia la API dentro del
contenedor Production.

Desde la raíz:

```powershell
.\Start-Development.ps1
```

El script crea `FunkoApiV.2\.env` desde `.env.example` si no existe, inicia
`db`, `mongo_db`, `cassandra`, `redis` y Mailpit, y arranca la API con el
entorno Development en el puerto `5798`. El proceso queda en primer plano;
detén la API con `Ctrl+C`. Consulta los mensajes de prueba en
<http://localhost:8025>.

Si necesitas iniciar o detener solo los almacenes manualmente:

```powershell
docker compose --env-file .\FunkoApiV.2\.env `
  --profile development -f .\FunkoApiV.2\compose.yaml `
  up -d db mongo_db cassandra redis mailpit

docker compose --env-file .\FunkoApiV.2\.env `
  --profile development -f .\FunkoApiV.2\compose.yaml `
  stop db mongo_db cassandra redis mailpit
```

La interpolación Compose valida `POSTGRES_PASSWORD` y `JWT_KEY` aunque se
seleccionen solo servicios de datos. Asegúrate de que exista
`FunkoApiV.2\.env`; el script de desarrollo lo crea desde la plantilla. Los
valores de `.env.example` son exclusivamente locales.

## 3. Arrancar el stack Compose completo

El servicio `backend` configura la API como **Production**. Si aún no tienes
`.env`, créalo desde la plantilla:

```powershell
Copy-Item .\.env.example .\FunkoApiV.2\.env
```

Antes de ejecutar el backend, edita `FunkoApiV.2\.env` y sustituye
`POSTGRES_PASSWORD` y `JWT_KEY` por valores privados aleatorios, y configura
`SMTP_HOST`, `SMTP_PORT`, `SMTP_SECURITY`, `SMTP_FROM`, `SMTP_USERNAME` y
`SMTP_PASSWORD` para tu proveedor. No compartas ni commitees ese fichero.
Las credenciales SMTP de ejemplo son marcadores, no valores válidos.

> Si una credencial SMTP real llegó a quedar en una rama o commit, revócala en
> el proveedor y genera otra. Borrarla del archivo no invalida una credencial
> que ya se haya expuesto.

> **PELIGRO: el backend Production borra datos al arrancar.** Cada inicio del
> servicio `backend` limpia tablas de aplicación PostgreSQL, proveedores
> MongoDB, tablas Cassandra y las claves de caché Redis con prefijos `funko:*`,
> `categoria:*` y `proveedor:*`. No carga cuentas de ejemplo. Un reinicio o
> recreación del backend vuelve a ejecutar la limpieza aunque los volúmenes
> persistan. No uses este comando para datos que quieras conservar.

Si aceptas expresamente ese comportamiento y solo para validar el despliegue:

```powershell
docker compose --env-file .\FunkoApiV.2\.env `
  -f .\FunkoApiV.2\compose.yaml up -d --build

docker compose --env-file .\FunkoApiV.2\.env `
  -f .\FunkoApiV.2\compose.yaml logs -f backend
```

El backend escucha en `http://localhost:5197`. Swagger y la interfaz visual de
GraphQL no se publican en Production. El endpoint GraphQL sigue disponible para
clientes API. El registro crea usuarios `USER`; el proceso no aprovisiona una
cuenta `ADMIN`.

Para detener servicios sin borrar volúmenes:

```powershell
docker compose --env-file .\FunkoApiV.2\.env `
  -f .\FunkoApiV.2\compose.yaml down
```

No uses `down -v` salvo que realmente quieras borrar también los volúmenes
persistentes de Docker.

## 4. Qué hacen los entornos al iniciar

La selección se basa en `IWebHostEnvironment` en `Program.cs`:

- **Development:** inicializa los esquemas, ejecuta `DbSeeder`,
  `CassandraSeeder` e `IdentitySeeder`. Los seeds se comprueban antes de
  insertarlos. El admin de ejemplo se define en `Seeders/IdentitySeeder.cs`.
- **Production y otros entornos no Development:** inicializa esquemas, ejecuta
  `ProductionDataCleaner.ClearAsync` y asegura los roles `ADMIN` y `USER`; no
  inserta usuarios de prueba.

El cleaner elimina los usuarios, pero conserva las definiciones de roles. El
registro público solo asigna rol `USER`; la creación controlada de un
administrador debe resolverse antes de desplegar una API utilizable.

## 5. Configuración de conexión

- `FunkoApiV.2/appsettings.json`: valores compartidos; las claves privadas se
  suministran por variables de entorno.
- `FunkoApiV.2/appsettings.Development.json`: servicios en `localhost`,
  Cassandra en el puerto `9043` y clave JWT local.
- `FunkoApiV.2/appsettings.Production.json`: hostnames Docker (`db`,
  `mongo_db`, `redis`, `cassandra`).
- `FunkoApiV.2/.env`: credenciales Compose privadas, excluidas de Git.

Las variables de entorno prevalecen sobre JSON. Compose transforma `JWT_KEY` en
`Jwt__Key`, utiliza `POSTGRES_PASSWORD` para PostgreSQL y el backend, y pasa las
variables `SMTP_*` a `Smtp__*`.

## 6. Comprobaciones y problemas

```powershell
docker compose --env-file .\FunkoApiV.2\.env `
  -f .\FunkoApiV.2\compose.yaml ps
docker compose --env-file .\FunkoApiV.2\.env `
  -f .\FunkoApiV.2\compose.yaml logs db
```

- **`compose.yaml` no encontrado:** usa la ruta `.\FunkoApiV.2\compose.yaml`
  desde la raíz; el archivo no está en la raíz.
- **Variables requeridas ausentes:** crea `FunkoApiV.2\.env` desde
  `.env.example` y revisa que no esté vacío.
- **Puerto ocupado:** comprueba qué proceso usa el puerto antes de detenerlo.
- **Pruebas de integración:** requieren Docker activo, aunque no requieren que
  los servicios del Compose local estén en marcha; Testcontainers crea los
  contenedores que necesita.

Para el flujo de arranque paso a paso, consulta
[la guía práctica](00-guia-practica.md).
