# 🚀 FunkoApi

API REST para gestión de Funkos, Categorías y Proveedores construida con .NET 10, C# 14 y arquitectura Onion-like híbrida.

> 📚 **Material de referencia**: Este proyecto sigue la guía de estilo de José Luis González Sánchez (`joseluisgs`) y los repositorios `TiendaDawApi-NetCore` y `DesarrolloWebEntornosServidor-02-2026-2027`.

---

## 📑 Tabla de Contenidos

- [📝 Descripción](#-descripción)
- [✨ Características](#-características)
- [🛠️ Tecnologías](#-tecnologías)
- [🗄️ Arquitectura de Datos](#-arquitectura-de-datos)
- [🚀 Inicio Rápido](#-inicio-rápido)
- [🧪 Estrategia de Testing](#-estrategia-de-testing)
- [📡 Endpoints](#-endpoints)
- [🔐 Seguridad](#-seguridad)
- [📦 Bruno Testing](#-bruno-testing)
- [🏗️ Estructura del Proyecto](#-estructura-del-proyecto)
- [📄 Documentación](#-documentación)
- [📄 Licencia](#-licencia)
- [✍️ Autores](#-autores)

---

## 📝 Descripción

API para gestionar una colección de Funkos (figuras de vinilo) con las siguientes bases de datos:

| Entidad | Base de datos | ID |
|---------|--------------|-----|
| **Funko** | PostgreSQL (EF Core) | Auto-incremental (int) |
| **Categoría** | PostgreSQL (EF Core) | Formato YouTube (11 caracteres) |
| **Proveedor** | MongoDB | ObjectId (24 caracteres hex) |

**Relaciones:**
- `Proveedor 1 — * Funko * — 1 Categoría`
- Un funko pertenece a una categoría y a un proveedor.
- **Borrado lógico**: al eliminar una categoría con funkos asociados, se devuelve `409 Conflict` y no se borran los funkos.

---

## ✨ Características

- ✅ CRUD completo de Funkos, Categorías y Proveedores
- ✅ Búsqueda paginada con filtros (nombre, categoría, proveedor, precio, stock)
- ✅ Borrado lógico (`IsDeleted`) con filtro global en EF Core
- ✅ Caché Redis con patrón Cache-Aside (solo por ID)
- ✅ Imágenes relativas (solo se guarda la ruta en BD)
- ✅ Patrón Result (`Result<T, DomainError>`) con CSharpFunctionalExtensions
- ✅ Validación con FluentValidation (mensajes en español)
- ✅ Mapeo manual entidad → DTO
- ✅ JWT + Autorización por roles (Admin para escritura)
- ✅ Middleware global de manejo de excepciones
- ✅ Tests unitarios (NUnit + Moq + FluentAssertions)
- ✅ Tests de integración (TestContainers + PostgreSQL)
- ✅ Colección Bruno automatizada con assertions
- ✅ Docker (PostgreSQL, MongoDB, Redis, API)
- ✅ Swagger/OpenAPI

---

## 🛠️ Tecnologías

| Tecnología | Versión | Uso |
|------------|---------|-----|
| .NET SDK | 10.0.401 | Runtime y compilación |
| C# | 14 | Lenguaje |
| ASP.NET Core | 10.0 | API Web |
| Entity Framework Core | 10.0.14 | ORM para PostgreSQL |
| Npgsql | 10.0.3 | Proveedor PostgreSQL |
| MongoDB.EntityFrameworkCore | 10.0.4 | MongoDB |
| MongoDB.Driver | 3.13.0 | MongoDB |
| StackExchange.Redis | 3.3.1 | Caché Redis |
| CSharpFunctionalExtensions | 3.7.0 | Patrón Result |
| FluentValidation | 11.3.1 | Validación de DTOs |
| AutoMapper | 16.2.0 | Mapeo (complemento) |
| BCrypt.Net-Next | 4.2.0 | Hash de contraseñas |
| Swashbuckle.AspNetCore | 10.2.3 | Swagger/OpenAPI |
| Serilog | 10.0.0 | Logging |
| NUnit | 4.6.1 | Tests unitarios |
| Moq | 4.21.0 | Mocks |
| FluentAssertions | 7.2.2 | Aserciones |
| Testcontainers | 4.15.0 | Tests de integración |

---

## 🗄️ Arquitectura de Datos

```
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│   PostgreSQL    │      │    MongoDB      │      │     Redis       │
│  (Funko, Cat.)  │      │   (Proveedor)   │      │   (Caché)       │
└────────┬────────┘      └────────┬────────┘      └────────┬────────┘
         │                        │                        │
         └────────────────────────┴────────────────────────┘
                                  │
                          ┌───────▼───────┐
                          │   FunkoApi    │
                          │   (.NET 10)   │
                          └───────┬───────┘
                                  │
                    ┌─────────────┴─────────────┐
                    │  HTTP / Swagger / Bruno   │
                    └───────────────────────────┘
```

**Tablas PostgreSQL:**
- `funko` (id, name, precio, stock, imagen, categoria_id, proveedor_id, created_at, updated_at, is_deleted)
- `categoria` (id youtube 11 chars, nombre, descripcion, created_at, updated_at, is_deleted)

**Colección MongoDB:**
- `proveedor` (_id ObjectId, nombre, direccion, created_at, updated_at, is_deleted)

**Claves de caché Redis:**
- `funko:{id}` — TTL configurable

---

## 🚀 Inicio Rápido

### Desarrollo Local

```powershell
# 1. Levantar servicios de base de datos (Docker)
docker-compose -f compose.yaml up -d

# 2. Ejecutar la API
dotnet run --project FunkoApiV.2\FunkoApiV.2.csproj

# 3. Abrir Swagger
# https://localhost:7259/openapi/v1.json o http://localhost:5197/openapi/v1.json
```

### Desarrollo en Producción con Docker

```powershell
# Construir y levantar todo el stack
docker-compose -f docker-compose.prod.yml up -d --build
```

### Variables de entorno (.env)

Copiar `.env.example` a `.env` y ajustar los valores:

```env
ConnectionStrings__DefaultConnection=Host=localhost;Database=funko;Username=admin;Password=admin
ConnectionStrings__Mongo=mongodb://localhost:27017
Cache__Redis=localhost:6379
Jwt__Key=clave-secreta-de-al menos-32-caracteres
Jwt__Issuer=FunkoApi
Jwt__Audience=FunkoApi
```

---

## 🧪 Estrategia de Testing

### Ejecución de Tests

```powershell
# Todos los tests
dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj

# Solo unitarios (rápidos, sin Docker)
dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj --filter "FullyQualifiedName~Unit"

# Solo integración (requiere Docker)
dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj --filter "FullyQualifiedName~Integration"

# Con cobertura
dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj --collect:"XPlat Code Coverage"
```

### Categorías de tests

| Categoría | Descripción |
|-----------|-------------|
| `Unit` | Tests unitarios rápidos (mocks, sin BD) |
| `Integration` | Tests de integración con TestContainers (PostgreSQL real) |
| `Repository` | Tests de repositorios sobre PostgreSQL |
| `Service` | Tests de servicios de dominio |
| `Controller` | Tests de controladores API |
| `Validator` | Tests de validaciones FluentValidation |
| `Dto` | Tests de DTOs y mapeo |
| `Cache` | Tests de caché Redis |
| `Docker` | Tests que requieren Docker |

---

## 📡 Endpoints

### Funkos (`/api/funkos`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `/api/funkos` | Listar funkos paginados con filtros | Público |
| GET | `/api/funkos/{id}` | Obtener funko por ID | Admin |
| POST | `/api/funkos` | Crear funko | Admin |
| PUT | `/api/funkos/{id}` | Actualizar funko | Admin |
| DELETE | `/api/funkos/{id}` | Eliminar funko (borrado lógico) | Admin |
| POST | `/api/funkos/imagen` | Subir imagen de funko | Público |
| PATCH | `/api/funkos/{id}/imagen` | Actualizar imagen de funko | Público |

**Filtros GET:** `nombre`, `categoriaId`, `proveedorId`, `precioMin`, `precioMax`, `stockMin`, `page`, `size`, `sortBy`, `direction`

### Categorías (`/api/categorias`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `/api/categorias` | Listar categorías paginadas | Público |
| GET | `/api/categorias/{id}` | Obtener categoría por ID | Público |
| POST | `/api/categorias` | Crear categoría | Admin |
| PUT | `/api/categorias/{id}` | Actualizar categoría | Admin |
| DELETE | `/api/categorias/{id}` | Eliminar categoría (borrado lógico) | Admin |

### Proveedores (`/api/Proveedor`) — MongoDB

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `/api/Proveedor` | Listar proveedores | Público |
| GET | `/api/Proveedor/{id}` | Obtener proveedor por ID | Público |
| POST | `/api/Proveedor` | Crear proveedor | Admin |
| PUT | `/api/Proveedor/{id}` | Actualizar proveedor | Admin |
| DELETE | `/api/Proveedor/{id}` | Eliminar proveedor | Admin |

---

## 🔐 Seguridad

- **Autenticación**: JWT Bearer
- **Autorización**: Política `RequireAdminRole` (rol `Admin`) para endpoints de escritura (POST, PUT, DELETE)
- **Endpoints públicos**: GET (listar y obtener por ID)
- **Hash de contraseñas**: BCrypt
- **Headers de seguridad**: X-Content-Type-Options, X-Frame-Options, Referrer-Policy, CSP
- **HTTPS/HSTS**: Solo en producción

---

## 📦 Bruno Testing

La colección Bruno está en la carpeta `Bruno/` y contiene **32 peticiones con pruebas automatizadas** que cubren los endpoints con casos positivos, negativos y de error.

### Ejecutar con script de PowerShell

Define `FUNKO_ADMIN_EMAIL` y `FUNKO_ADMIN_PASSWORD` en el entorno local con las credenciales de una cuenta `ADMIN`; no las guardes en `Local.bru`.

```powershell
.\Run-Bruno.ps1
```

O ejecutar manualmente:

```powershell
# App Bruno: abrir la carpeta Bruno y ejecutar "Run Collection" con entorno Local
# CLI:
Push-Location .\Bruno
bru run -r --env-file .\environments\Local.bru
Pop-Location
```

### Flujo recomendado en Bruno

1. `0 - Setup/01 - Crear proveedor` (captura `proveedorIdCreado`)
2. `2 - Funkos/01 - Listar funkos` → `03 - Crear funko` → `04 - Actualizar` → `05 - Eliminar`
3. `1 - Categorias/01 - Listar` → `02 - Crear` → `03 - Eliminar con funkos (409)` → `04 - Eliminar creada`
4. `3 - Proveedores/01 - Listar` → `02 - Obtener por id` → `03 - Actualizar`
5. `90 - Teardown/01 - Eliminar proveedor`

---

## 🏗️ Estructura del Proyecto

```
FunkoApiV.2/
├── FunkoApiV.2.slnx              # Solución moderna (.NET 10)
├── FunkoApiV.2/                  # Proyecto API
│   ├── Controllers/              # Controladores (Funko, Categoria, Proveedor, Auth)
│   ├── Services/                 # Servicios de negocio (con patrón Result)
│   ├── Repositories/             # Repositorios (EF Core PostgreSQL + MongoDB)
│   ├── Models/                   # Entidades de dominio
│   ├── Dtos/                     # DTOs (Request, Response, Filter)
│   ├── Mappers/                  # Mapeadores entidad ↔ DTO
│   ├── Validators/               # Validators FluentValidation
│   ├── Errors/                   # Errores de dominio (DomainError)
│   ├── Infrastructures/          # Configuración DI (extension methods)
│   ├── Middleware/               # GlobalExceptionHandler, SecurityHeaders
│   ├── Data/                     # DbContext (AppDbContext, AppDbContextMongoDb)
│   ├── Cache/                    # Servicio de caché Redis
│   ├── Storage/                  # Servicio de almacenamiento de imágenes
│   ├── Program.cs                # Punto de entrada y pipeline HTTP
│   ├── appsettings.json          # Configuración base
│   └── Dockerfile                # Imagen Docker multi-stage
├── FunkoApiV2Test/               # Proyecto de tests
│   ├── Unit/                     # Tests unitarios (Controllers, Services, Mappers, Validators)
│   └── Integration/              # Tests de integración (TestContainers PostgreSQL)
├── Bruno/                        # Colección Bruno automatizada
│   ├── 0 - Setup/
│   ├── 1 - Categorias/
│   ├── 2 - Funkos/
│   ├── 3 - Proveedores/
│   ├── 90 - Teardown/
│   └── environments/Local.bru
├── doc/                          # Documentación numerada (estilo profe)
├── compose.yaml                  # Docker Compose (Postgres, Mongo, Redis, API)
├── Dockerfile                    # Dockerfile de la API
├── .env.example                  # Plantilla de variables de entorno
└── README.md                     # Este archivo
```

---

## 📄 Documentación

La carpeta `doc/` contiene guías numeradas siguiendo el estilo del profesor:

| Documento | Descripción |
|-----------|-------------|
| `doc/01-configuracion-proyectos-dotnet.md` | Configuración de proyectos .NET |
| `doc/02-arquitectura-pipeline-http.md` | Arquitectura y pipeline HTTP |
| `doc/03-inyeccion-dependencias.md` | Inyección de dependencias |
| `doc/04-controladores-rest.md` | Controladores REST |
| `doc/05-validacion-cascada.md` | Validación en cascada |
| `doc/06-rest-best-practices.md | Mejores prácticas REST |
| `doc/07-repository-pattern.md` | Patrón Repositorio |
| `doc/08-ef-core-postgresql.md` | EF Core con PostgreSQL |
| `doc/09-mongodb.md` | MongoDB (Proveedores) |
| `doc/10-redis-caching.md` | Caché Redis |
| `doc/11-patron-result.md` | Patrón Result / ROP |
| `doc/12-servicios-negocio.md` | Servicios de dominio |
| `doc/13-borrado-logico.md` | Borrado lógico y reglas de negocio |
| `doc/14-mapeadores.md` | Mapeadores entidad ↔ DTO |
| `doc/15-jwt-authentication.md` | Autenticación JWT |
| `doc/16-autorizacion-roles.md` | Autorización por roles |
| `doc/17-seguridad-http.md` | Seguridad HTTP |
| `doc/18-testing.md` | Estrategia de testing |
| `doc/19-docker.md` | Docker y Docker Compose |
| `doc/20-bruno-testing.md` | Testing con Bruno |

---

## 📄 Licencia

Este proyecto está basado en el material de **José Luis González Sánchez** (`joseluisgs`), bajo licencia **Creative Commons BY-NC-SA 4.0**. Si reutilizas fragmentos o ideas, debes citar al autor.

---

## ✍️ Autores

- **Tu nombre** — Funko/Categoría (PostgreSQL)
- **Compañero** — Proveedor (MongoDB)

---

> 💡 **Tip**: Para ejecutar todo el stack con un solo comando, usa `docker-compose -f compose.yaml up -d`. Para ejecutar los tests Bruno, usa `.\Run-Bruno.ps1`.