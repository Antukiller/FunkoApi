# Presentación mejorada — FunkoApi

Auditoría de la presentación `FunkoApi-Canva.pptx` (10 diapositivas) contra el código real de la rama `dev`.

- **Repositorio:** `D:\Ejercicios 2DAW\FunkoApi` (rama `dev`)
- **PPTX auditado:** `C:\Users\antoi\Downloads\FunkoApi-Canva.pptx`
- **Duración objetivo:** 12 min 30 s (límite 15 min)
- **Regla de estilo:** 5–6 palabras por viñeta, una idea por diapositiva

---

## 1. Verificación de datos contra el código

| Afirmación de la presentación | Lo que hay en el código | Veredicto |
|---|---|---|
| 286 tests | `dotnet test --list-tests` enumera **286** casos | ✅ Correcto |
| 34 peticiones de Bruno | **34** ficheros `.bru` de petición (sin contar el entorno) | ✅ Correcto |
| 80 comprobaciones Bruno | **80** bloques `test()` y **84** `expect()` | ✅ Correcto |
| Redis con TTL de 30 minutos | `Cache:ExpiryMinutes = 30` **y** `SlidingExpirationMinutes = 60` | ⚠️ Incompleto |
| Lectura pública y escritura ADMIN | Listado de funkos y categorías públicos; **ficha por id solo ADMIN**; **proveedores todo ADMIN** | ❌ Incorrecto |
| JWT con roles | Claim `ClaimTypes.Role` en el token + políticas `RequireAdmin` / `RequireUser` | ✅ Correcto |
| SignalR en `/hubs/funkos` | `MapHub<FunkosHub>("/hubs/funkos").RequireAuthorization()` | ✅ Correcto |
| Evento `FunkoCreado` | `FunkoService` lo envía **solo al grupo `admins`** | ⚠️ Matizar |
| Mutaciones GraphQL con `RequireAdmin` | Las 4 mutaciones llevan `[Authorize(Policy = "RequireAdmin")]` | ✅ Correcto |
| Suscripciones GraphQL | 4 declaradas; `onStockBajo` **nunca se publica** | ⚠️ Matizar |
| PostgreSQL / MongoDB / Cassandra / Redis | Confirmado por `AppDbContext`, `AppDbContextMongoDb`, esquema Cassandra y claves de caché | ✅ Correcto |
| CSP y HSTS en Production | CSP y cabeceras sí; **HSTS no existe** en el código | ❌ Incorrecto |
| Middleware global de errores | **No hay** middleware de excepciones; cada controlador mapea `Result` a HTTP | ❌ Incorrecto |
| Un comando arranca todo | `Start.ps1` sí; pero son **varios contenedores**, no uno | ⚠️ Matizar |

### Detalle de cada verificación

**Tests (286).** El listado de `dotnet test --list-tests` devuelve exactamente 286 nombres. Ojo: los tests de integración usan Testcontainers y `DockerTestPrerequisite` hace `Assert.Ignore` si Docker no está levantado, así que el número baja si se ejecutan sin Docker.

**Bruno (34 / 80).** 34 peticiones repartidas en 5 carpetas (`0 - Setup`, `1 - Categorias`, `2 - Funkos`, `3 - Proveedores`, `90 - Teardown`). 80 `test()` y 84 `expect()`.

**Redis.** `appsettings.json`: `Cache.ExpiryMinutes = 30` y `Cache.SlidingExpirationMinutes = 60`. `RedisService` aplica expiración absoluta **y** deslizante. Solo se cachean fichas por id: `funko:{id}`, `categoria:{id}`, `proveedor:{id}`, `proveedor:all`, `proveedor:deleted`. Los listados paginados de funkos **no** se cachean.

**Permisos reales.**

| Recurso | Lectura | Escritura |
|---|---|---|
| `GET /api/funkos` (listado) | Pública | — |
| `GET /api/funkos/{id}` | **ADMIN** | — |
| `POST/PUT/DELETE /api/funkos` | — | ADMIN |
| Categorías (GET) | Pública | ADMIN |
| Proveedores (todo) | **ADMIN** | ADMIN |
| `/api/cassandra/*` | Pública | — |
| `GET /api/auth/me` | Autenticado | — |

**SignalR.** `FunkosHub` lleva `[Authorize]`. En `OnConnectedAsync` añade al grupo `admins` si el usuario tiene el rol. `FunkoService.NotificarFunkoCreado` envía `FunkoCreado` a `Clients.Group("admins")`, no a todos. El hub tiene además `SendMessage` restringido a ADMIN.

**GraphQL.** `GraphQLConfig` activa `AddAuthorization()`, `AddInMemorySubscriptions()` y habilita el explorador solo en Development. Las mutaciones publican `onFunkoCreado`, `onFunkoActualizado`, `onFunkoEliminado` y `FunkoImagenActualizada`. `onStockBajo` está declarada en `FunkoSubscriptions` pero no se publica en ningún sitio.

**Bases de datos.**

| Almacén | Contenido real |
|---|---|
| PostgreSQL | `Funko`, `Categoria`, Identity (`AppUser`, roles) |
| MongoDB | `Proveedor` (documento con `ObjectId`) |
| Cassandra | 3 tablas de proyección: `funkos_por_categorias`, `funkos_por_proveedor`, `funkos_por_proveedor_categoria` |
| Redis | Caché de lectura por id (fichas y listas de proveedores) |

**Producción.** En `Program.cs`, fuera de Development solo se ejecuta `IdentitySeeder.EnsureProductionAdminAsync`; no se siembran datos ni se borra nada. Las cabeceras que sí se añaden en Production son `Content-Security-Policy`, `X-Content-Type-Options`, `X-Frame-Options` y `Referrer-Policy`. **No hay `UseHsts()`**.

**Errores.** No existe middleware global de excepciones. `DomainErrorExtensions.TopHttpResult` traduce `Result` a códigos HTTP en cada controlador (404, 400, 409, 422, 401, 500), y `InvalidModelStateResponseFactory` unifica los 400 de validación.

---

## 2. Guion diapositiva a diapositiva

### 1 · Portada — 0:30

**Texto en pantalla**
- FunkoApi
- API de catálogo de Funkos
- Antoine López · 2º DAW
- Desarrollo Web en Entornos Servidor
- github.com/Antukiller/FunkoApi · rama `dev`

**Elemento visual:** la portada actual (fondo oscuro + figura Funko + portátil). Funciona; no la toques.

**Guion**
> Buenas. Soy Antoine López, de 2º DAW. Voy a presentar FunkoApi, una API de catálogo de Funkos hecha con .NET 10. Está en la rama `dev` del repositorio que veis abajo. No voy a contar solo qué hace: voy a enseñar cómo está montada y qué decisiones hay detrás.

**Tiempo:** 0:30

---

### 2 · Problema y objetivo — 1:00

**Texto en pantalla**
- Un catálogo, tres formas de acceso
- REST, GraphQL y SignalR
- Mismas reglas, un solo dominio
- Datos aislados en repositorios
- Un comando arranca todo

**Elemento visual:** tres iconos (REST, GraphQL, SignalR) confluyendo en una caja "Dominio". Debajo, una línea con `.\Start.ps1`.

**Guion**
> El problema de partida era simple: un catálogo no se resuelve bien con una sola forma de consultar. Necesitaba REST para las operaciones, GraphQL para que el cliente pida solo los campos que quiere y SignalR para avisar de cambios en vivo. Lo importante es que los tres hablan con el mismo dominio y las mismas reglas, y que el acceso a datos está detrás de repositorios. Todo se levanta con un comando.

**Tiempo:** 1:00

---

### 3 · Stack — 1:00

**Texto en pantalla**
- .NET 10 · EF Core 10
- PostgreSQL · MongoDB · Cassandra · Redis
- REST · GraphQL · SignalR
- JWT · FluentValidation · Serilog
- Docker Compose

**Elemento visual:** el stack de placas 3D actual, cambiando la placa de APIs a `REST · GraphQL · SignalR`.

**Guion**
> El stack es .NET 10 con EF Core 10 y cuatro motores de datos. En la capa de API conviven REST con Swagger, GraphQL con HotChocolate y SignalR, que usa WebSocket como transporte. Añadí JWT, FluentValidation y Serilog, y todo se orquesta con Docker Compose.

**Tiempo:** 1:00

---

### 4 · Arquitectura — 1:15

**Texto en pantalla**
- Cliente → Controller → Service → Repository
- Controller: contrato HTTP y DTO
- Service: reglas de negocio y Result
- Repository: acceso a datos
- Program.cs compone; Infrastructure agrupa

**Elemento visual:** el diagrama actual de cajas (CLIENTE, CONTROLLER, SERVICE, REPOSITORY, BASE DE DATOS). Elimina las etiquetas laterales largas.

**Guion**
> Una petición entra por el controlador, que define el contrato HTTP y trabaja con DTO. El servicio aplica las reglas y devuelve un `Result`: nada de lanzar excepciones para errores de negocio. El repositorio encapsula el acceso a datos, y `Program.cs` compone todo con extensiones en `Infrastructure`. Cada capa se puede probar por separado.

**Tiempo:** 1:15

---

### 5 · Modelo de datos y coherencia — 1:15

**Texto en pantalla**
- PostgreSQL: Funkos, categorías, usuarios
- MongoDB: proveedores, documento flexible
- Cassandra: agregados por categoría y proveedor
- Redis: caché de fichas por id
- PostgreSQL manda; Cassandra replica

**Elemento visual:** el diagrama de cuatro cilindros actual. Marca PostgreSQL como "fuente de verdad" y Cassandra como "proyección".

**Guion**
> Cada dato vive donde le corresponde. PostgreSQL guarda Funkos, categorías y usuarios, porque necesito integridad y relaciones. MongoDB guarda proveedores, que son documentos con forma variable. Cassandra tiene tres tablas ya agregadas por categoría, por proveedor y por la combinación, para consultas directas. Y Redis cachea fichas por id.
>
> ¿Y la coherencia? PostgreSQL es la fuente de verdad. Cuando creo un Funko, primero se escribe en PostgreSQL y después `FunkoService` sincroniza las tres tablas de Cassandra con los nombres ya desnormalizados. Si en una actualización cambia la categoría o el proveedor, borra las filas antiguas y escribe las nuevas. Al borrar, hace borrado lógico en PostgreSQL y elimina las proyecciones. Redis se invalida en cada escritura. No hay transacción distribuida: es consistencia eventual y lo asumo.

**Tiempo:** 1:15

---

### 6 · Endpoints REST — 1:30

**Texto en pantalla (tabla, no imagen)**
| Método | Ruta | Acceso |
|---|---|---|
| POST | `/api/auth/login` | Público |
| GET | `/api/funkos` | Público, paginado |
| GET | `/api/funkos/{id}` | ADMIN |
| POST · PUT · DELETE | `/api/funkos` | ADMIN |
| GET | `/api/proveedores` | ADMIN |
| GET | `/api/cassandra/...` | Público |

Pie: `page` empieza en 0 · `size` 1–100 · máx. 100

**Elemento visual:** tabla real de PowerPoint o Canva, con texto editable. **La diapositiva 6 actual está rasterizada**: todo su contenido es una imagen y en el PPTX no hay ni una sola letra seleccionable. Hay que rehacerla.

**Guion**
> Aquí están las rutas principales. El login es público y devuelve el JWT. El listado de Funkos es público y paginado: la página empieza en cero, el tamaño por defecto es diez y el máximo cien. La ficha por id y todas las escrituras requieren ADMIN. Proveedores es ADMIN completo. Y las consultas agregadas de Cassandra son públicas.

**Tiempo:** 1:30

---

### 7 · GraphQL y tiempo real — 1:30

**Texto en pantalla**
- `{ funkos { items { id name } } }`
- Consultas públicas; mutaciones ADMIN
- Suscripciones: creado, actualizado, eliminado
- SignalR autenticado en `/hubs/funkos`
- `FunkoCreado` solo para administradores

**Elemento visual:** el diagrama neón actual (Query / Mutation / Subscription / SignalR). Mantén la query; elimina una viñeta de la columna izquierda.

**Guion**
> GraphQL reutiliza el mismo dominio. Esta consulta de una línea devuelve los Funkos con los campos que pido. Las consultas son públicas y las cuatro mutaciones exigen el rol ADMIN. Las suscripciones avisan de creado, actualizado y eliminado. En paralelo, SignalR escucha en un hub autenticado y emite `FunkoCreado`, pero solo al grupo de administradores conectados. La suscripción `onStockBajo` está declarada, aunque todavía no se publica: no la vendáis como funcionando.

**Tiempo:** 1:30

---

### 8 · Seguridad y rendimiento — 1:30

**Texto en pantalla**
- JWT con roles y ClockSkew cero
- Rate limit: 120/min por IP
- CORS por entorno, con credenciales
- CSP y cabeceras en Production
- Índices GIN trigram y compuestos
- Redis: 30 min absoluta, 60 deslizante

**Elemento visual:** cuatro bloques con icono: Autenticación, Límites, Datos, Caché. Nada de frases genéricas.

**Guion**
> Cuatro cosas concretas. Autenticación: JWT con el rol en el token, validación de emisor, audiencia y caducidad, y desfase de reloj a cero. Límites: 120 peticiones por minuto y por IP; al pasarse devuelve 429 con `Retry-After`. CORS: orígenes configurados y, solo en desarrollo, los locales; con credenciales porque SignalR lo necesita. Producción añade CSP, `X-Frame-Options: DENY`, `nosniff` y `Referrer-Policy`. Ojo: HSTS no está implementado, no lo digáis. En datos, índices GIN con trigramas para buscar por nombre y compuestos para los filtros. Y Redis cachea la ficha treinta minutos, con expiración deslizante de sesenta.

**Tiempo:** 1:30

---

### 9 · Demo en vivo — 2:00

**Texto en pantalla**
- Login admin → token
- GET funkos con filtros
- POST funko (ADMIN) → 201
- SignalR avisa `FunkoCreado`
- Plan B: Bruno y capturas

**Elemento visual:** tres ventanas: Swagger, terminal y la página `/` del hub. Si no hay proyector cómodo, captura de las tres.

**Guion (paso a paso, 2 minutos)**
> 1. Abro Swagger en `http://localhost:5798/swagger` y ejecuto `POST /api/auth/login` con la cuenta admin de desarrollo. Copio el campo `token` y pulso Authorize, sin escribir `Bearer`.
> 2. Ejecuto `GET /api/funkos?page=0&size=5&sortBy=precio&direction=desc`. Comento la respuesta: `items`, `totalCount`, `totalPages` y que la página empieza en cero.
> 3. Abro `/` en otra pestaña, inicio sesión como admin y conecto el hub. Dejo esa pestaña visible.
> 4. Vuelvo a Swagger y hago `POST /api/funkos` con un nombre corto, precio entre 0,01 y 50, stock, una categoría de 11 caracteres, un proveedor de 24 hexadecimales y una ruta de imagen. Espero 201.
> 5. Señalo la notificación `FunkoCreado` en la otra pestaña: solo llega a administradores.

**Plan B**
- Si Docker o las bases tardan: no cambies a Production (no tiene datos ni Swagger). Espera a `Now listening on: http://localhost:5798`.
- Si falla una escritura: vuelve al `GET` ya probado y sigue con GraphQL en `/graphql`.
- Si no hay internet, la página `/` no carga SignalR porque usa CDN: enseña una captura o el test del hub.
- Si no hay Docker, los tests de integración se ignoran: no ejecutes `dotnet test` en vivo, enseña el resultado ya guardado.
- Último recurso: la colección Bruno (34 peticiones) o capturas preparadas.

**Tiempo:** 2:00

---

### 10 · Conclusiones — 1:00

**Texto en pantalla**
- **286** tests · **34** peticiones Bruno
- Aprendí: capas, Result, cuatro almacenes
- Coste real: coherencia entre bases
- Mejoras: outbox, Output Cache, CI
- **¿Preguntas?**

**Elemento visual:** `286` en tipografía enorme, con `34 / 80` debajo en pequeño; luego tres líneas y "¿Preguntas?" centrado.

**Guion**
> Cierro con lo importante: 286 tests automatizados y 34 peticiones de Bruno con 80 comprobaciones. Lo que me llevo es haber entendido las capas, el patrón Result y el coste de usar cuatro almacenes: la coherencia no es gratis. Como mejoras: una cola outbox para reintentar la sincronización con Cassandra, Output Cache para las listas y un pipeline de CI. Y hasta aquí: ¿preguntas?

**Tiempo:** 1:00

---

### Resumen de tiempos

| # | Diapositiva | Tiempo |
|---|---|---|
| 1 | Portada | 0:30 |
| 2 | Problema y objetivo | 1:00 |
| 3 | Stack | 1:00 |
| 4 | Arquitectura | 1:15 |
| 5 | Modelo de datos y coherencia | 1:15 |
| 6 | Endpoints REST | 1:30 |
| 7 | GraphQL y tiempo real | 1:30 |
| 8 | Seguridad y rendimiento | 1:30 |
| 9 | Demo | 2:00 |
| 10 | Conclusiones | 1:00 |
| | **Total** | **12:30** |

---

## 3. Cambios y por qué

1. **Stack: añadido REST y unificado el término.** `GraphQL · WebSocket` se queda corto y mezcla protocolo con transporte. Ahora: `REST · GraphQL · SignalR`. El código usa SignalR, no WebSocket crudo; WebSocket es solo el transporte que negocia.
2. **Arquitectura reducida a 5 ideas.** Antes había 4 frases largas y etiquetas laterales; ahora una línea por capa. Se explica igual en 20 segundos menos.
3. **Modelo de datos con la coherencia incluida.** La diapositiva 5 ya no muestra ids: muestra las cuatro bases y su papel, y añade la frase que faltaba: PostgreSQL manda y Cassandra replica. La explicación del *cómo* va en el guion.
4. **Endpoints corregidos.** La diapositiva decía "lectura pública". En realidad el detalle por id y los proveedores son ADMIN. La tabla lo refleja y añade la paginación real (base 0, tamaño 10, máximo 100).
5. **GraphQL sin duplicar SignalR.** Se mantiene la query simple y se aclara que `FunkoCreado` va solo al grupo `admins`.
6. **Seguridad con datos, no adjetivos.** Fuera "seguridad sólida", "caché inteligente" e "índices y consultas". Dentro: 120/min por IP, ClockSkew 0, GIN trigram, 30/60 minutos, CSP.
7. **Demo con guion y plan B.** La diapositiva era estática; ahora hay cinco pasos, códigos esperados y cinco alternativas si falla algo.
8. **Conclusión reforzada.** Cifra grande, aprendizaje, coste real, mejoras y cierre con "¿Preguntas?".
9. **Diapositiva 6 rehacer.** Está rasterizada: en el PPTX no hay texto editable. Hay que reconstruirla como tabla.

---

## 4. Discrepancias entre la presentación y el código real

**Graves (corregir antes de exponer)**
- **"Lectura pública y escritura ADMIN"**: `GET /api/funkos/{id}` exige ADMIN y todo `/api/proveedores` exige ADMIN. Solo el listado de Funkos, las categorías y Cassandra son públicos.
- **"CSP y HSTS en Production"**: HSTS no está implementado. Solo hay CSP y cabeceras de seguridad.
- **"Middleware global de errores"**: no existe. Los errores se traducen en cada controlador con `TopHttpResult` y `InvalidModelStateResponseFactory`.
- **Diapositiva 6 rasterizada**: no se puede editar ni leer como texto en el PPTX.

**Menores (matizar)**
- **Redis "30 minutos"**: son 30 minutos absolutos **más** 60 deslizantes.
- **`FunkoCreado`**: se envía al grupo `admins`, no a todos los clientes.
- **`onStockBajo`**: declarada pero nunca publicada.
- **"Un solo comando"**: correcto; "un solo contenedor", no. En producción son cinco servicios.
- **286 tests**: correcto con Docker levantado; sin Docker, los de integración se ignoran.
- **Diapositiva 10 actual**: solo tiene tres viñetas; faltan Testcontainers y próximos pasos, que sí estaban en el diseño anterior.

**Correcto y conviene mantener**
- 286 tests y 34/80 en Bruno.
- JWT con roles y políticas.
- SignalR en `/hubs/funkos` con hub autenticado.
- Mutaciones GraphQL con `RequireAdmin`.
- Reparto PostgreSQL / MongoDB / Cassandra / Redis.

---

## 5. Seis preguntas difíciles y respuesta modelo

### 1. ¿Por qué cuatro almacenes y no solo PostgreSQL?

> Porque cada dato tiene una forma de uso distinta. Funkos y categorías son relacionales y necesitan integridad, así que van en PostgreSQL junto con Identity. Los proveedores son documentos con estructura variable: MongoDB evita tablas vacías o columnas de relleno. Cassandra sirve las consultas agregadas por categoría y proveedor con tablas ya ordenadas por partición, sin *joins*. Y Redis evita ir a disco para la ficha por id. No es capricho: cada motor responde a una consulta concreta del proyecto.

### 2. ¿Cómo evitas inconsistencias entre PostgreSQL, MongoDB y Cassandra?

> PostgreSQL es la fuente de verdad. En `FunkoService`, cuando creo un Funko valido primero que existan la categoría y el proveedor, escribo en PostgreSQL y después sincronizo las tres tablas de Cassandra con los nombres desnormalizados. En una actualización, si cambia la categoría o el proveedor, borro las filas antiguas de las tres tablas y escribo las nuevas. Al borrar, hago borrado lógico en PostgreSQL y elimino las proyecciones. Redis se invalida en cada escritura. No uso transacción distribuida: es consistencia eventual. Y sé dónde duele: si Cassandra falla justo después del commit, no hay reintento; y si renombro una categoría, el nombre viejo se queda en Cassandra. La mejora pendiente es un patrón outbox con reintentos y un reconciliador.

### 3. ¿Por qué REST y GraphQL a la vez? ¿No es duplicar?

> No, porque comparten servicios y repositorios: lo que cambia es el contrato. REST me da semántica HTTP, códigos claros, Swagger y es lo que usaría cualquier cliente o integración. GraphQL me deja pedir solo los campos necesarios, resolver en una petición lo que en REST serían varias, y ofrece suscripciones. La lógica de negocio está una sola vez, en los servicios; los dos adaptadores solo traducen.

### 4. Si ya tienes Redis, ¿por qué no Output Cache?

> Porque no es lo mismo. Redis aquí es caché de aplicación: guardo la ficha de un Funko por id y la invalidó cuando ese Funko cambia. Output Cache cachearía la respuesta HTTP completa, con su query string; en listados filtrados y paginados la cardinalidad es altísima y la invalidación es mucho más difícil. No lo he implementado para no cachear datos equivocados. Si lo añadiera, sería con etiquetas por recurso y políticas por endpoint, no global.

### 5. ¿Cómo garantizas que solo un ADMIN escribe?

> Hay tres capas. En el token incluyo el claim de rol al hacer login. En REST, las escrituras llevan `[Authorize(Roles = "ADMIN")]`, y el controlador de proveedores lo lleva a nivel de clase entero. En GraphQL, las cuatro mutaciones usan la política `RequireAdmin`, definida en `IdentityConfig`. Y el hub de SignalR exige autenticación y añade al usuario al grupo `admins` solo si tiene el rol. Además, `includeDeleted` en el listado se comprueba en código y devuelve 403 si no eres ADMIN.

### 6. ¿Qué pasa si Cassandra cae justo cuando creas un Funko?

> Hoy: PostgreSQL ya ha confirmado la escritura, la sincronización con Cassandra se lanza después y, si falla, la excepción sube y el cliente recibe un 500 aunque el Funko exista. Es el punto débil real del diseño. La solución correcta es no hacer la sincronización en la petición: escribir un evento en una tabla outbox dentro de la misma transacción de PostgreSQL y que un proceso en segundo plano reintente contra Cassandra. Mientras tanto, los seeders permiten reconstruir las proyecciones, pero eso no es una solución en caliente.

---

## 6. Notas de producción

- **Término único:** usa **SignalR** en todas las diapositivas. Si el profesor exige la palabra "WebSocket", escribe una sola vez `SignalR (WebSocket)` en el stack y explica que WebSocket es el transporte.
- **Diapositiva 6:** rehacerla con tabla editable. La actual es una imagen completa.
- **Diapositiva 10:** recuperar las dos viñetas perdidas (Testcontainers y próximos pasos) o sustituirlas por las de este guion.
- **Diapositiva 8:** no digas HSTS ni "middleware global". No existen.
- **Credenciales de demo:** `admin@admin.com` / `Admin123!` solo en Development. No las enseñes en Production.
- **Datos de demo:** el `POST` necesita una categoría de 11 caracteres (`VQGCKyvzIM4` en el seed) y un proveedor de 24 hexadecimales; sácalo de `GET /api/proveedores`. La imagen es obligatoria en la creación.
- **No ejecutes tests en vivo** si no hay Docker: los de integración se ignoran y el número no cuadra.

---

*Documento generado a partir del código de la rama `dev` y del PPTX `FunkoApi-Canva.pptx`. No se ha modificado ningún fichero fuente.*
