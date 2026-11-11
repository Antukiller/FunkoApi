# Probar GraphQL en el navegador

## Abrir el explorador

1. Inicia la API desde el perfil de desarrollo.
2. Inicia bruno para obetener el token de autenticacion(http://localhost:5197/api/auth/login)
3. Abre `http://localhost:5197/graphql` (o `https://localhost:7259/graphql` si usas HTTPS).
4. En el explorador, escribe una operación en el panel izquierdo y las variables JSON en el panel **Variables**.

Los nombres de campos que aparecen aquí usan el formato GraphQL habitual de Hot Chocolate: camelCase. Las consultas de lectura no requieren autenticación. Las mutaciones requieren un token de un usuario con rol `ADMIN`.

Para obtener un token, inicia sesión con una cuenta existente enviando `POST http://localhost:5197/api/auth/login` con este cuerpo:

```json
{
  "email": "admin@admin.com",
  "password": "Admin123!"
}
```

Copia el valor `token` de la respuesta y configúralo en el panel **Headers** del explorador:

```json
{
  "Authorization": "Bearer TU_TOKEN"
}
```

## Consultas

### Listar todos los Funkos

No tiene variables:

```graphql
query {
  funkos {
    id
    name
    precio
    stock
    imagen
    categoria
    categoriaId
    proveedorId
    createdAt
    updatedAt
  }
}
```

### Buscar un Funko por ID

Variable: `id` (`Long!`).

```graphql
query FunkoPorId($id: Long!) {
  funkoById(id: $id) {
    id
    name
    precio
    stock
    imagen
    categoria
    categoriaId
    proveedorId
    createdAt
    updatedAt
  }
}
```

```json
{
  "id": 1
}
```

### Listar Funkos por página

Variables: `page` (`Int`, empieza en 1) y `size` (`Int`, máximo 100).

```graphql
query FunkosPaginados($page: Int!, $size: Int!) {
  funkosPaged(page: $page, size: $size) {
    items {
      id
      name
      precio
      stock
      categoria
      categoriaId
      proveedorId
    }
    totalCount
    page
    pageSize
    totalPages
    hasNextPage
    hasPreviousPage
  }
}
```

```json
{
  "page": 1,
  "size": 10
}
```

### Listar categorías por página

Variables: `page` (`Int`, empieza en 1) y `size` (`Int`, máximo 100).

```graphql
query CategoriasPaginadas($page: Int!, $size: Int!) {
  categoriasPaged(page: $page, size: $size) {
    items {
      id
      nombre
      descripcion
      createdAt
      updatedAt
    }
    totalCount
    page
    pageSize
    totalPages
    hasNextPage
    hasPreviousPage
  }
}
```

```json
{
  "page": 1,
  "size": 10
}
```

### Buscar una categoría por ID

Variable: `id` (`String!`). Usa un ID devuelto por `categoriasPaged`.

```graphql
query CategoriaPorId($id: String!) {
  categoriaById(id: $id) {
    id
    nombre
    descripcion
    createdAt
    updatedAt
  }
}
```

```json
{
  "id": "Peliculas01"
}
```

## Mutaciones

Los campos `categoriaId` y `proveedorId` deben corresponder a registros existentes. Los ID de categoría tienen 11 caracteres; el proveedor usa un ObjectId hexadecimal de 24 caracteres. Se pueden consultar categorías en GraphQL y proveedores en `GET /api/proveedor` (requiere token de administrador).

### Crear un Funko

Variable: `input` (`CreateFunkoInput!`).

```graphql
mutation CrearFunko($input: CreateFunkoInput!) {
  crearFunko(input: $input) {
    id
    name
    precio
    stock
    imagen
    categoria
    categoriaId
    proveedorId
    createdAt
    updatedAt
  }
}
```

```json
{
  "input": {
    "name": "Funko de prueba",
    "precio": 19.99,
    "stock": 5,
    "imagen": "https://ejemplo.com/funko.png",
    "categoriaId": "Peliculas01",
    "proveedorId": "672f1a8b3c9d4e5f6a7b8c9d"
  }
}
```

### Actualizar un Funko

Variables: `id` (`Long!`) y `input` (`UpdateFunkoInput!`). La actualización requiere también el ID del proveedor actual.

```graphql
mutation ActualizarFunko($id: Long!, $input: UpdateFunkoInput!) {
  actualizarFunko(id: $id, input: $input) {
    id
    name
    precio
    stock
    imagen
    categoria
    categoriaId
    proveedorId
    updatedAt
  }
}
```

```json
{
  "id": 1,
  "input": {
    "name": "Funko actualizado",
    "precio": 20.5,
    "stock": 8,
    "imagen": "https://ejemplo.com/funko-actualizado.png",
    "categoriaId": "Peliculas01",
    "proveedorId": "672f1a8b3c9d4e5f6a7b8c9d"
  }
}
```

### Eliminar un Funko

Variable: `id` (`Long!`). Usa un ID de prueba: la eliminación cambia el registro.

```graphql
mutation EliminarFunko($id: Long!) {
  eliminarFunko(id: $id)
}
```

```json
{
  "id": 1
}
```

### Actualizar la imagen

Variables: `id` (`Long!`) y `file` (`Upload!`). La variable `file` es un archivo, no texto JSON; selecciona una imagen en el control de carga del explorador. Si el explorador no ofrece ese control, su cliente debe soportar cargas GraphQL multipart.

```graphql
mutation ActualizarImagen($id: Long!, $file: Upload!) {
  actualizarImagenFunko(id: $id, file: $file) {
    id
    name
    imagen
  }
}
```

### Suscripciones: probar en Bruno

Las suscripciones usan WebSocket, no una petición GraphQL normal por HTTP. En Bruno crea una petición **WebSocket** conectada a `ws://localhost:5197/graphql` (o `wss://localhost:7259/graphql` con HTTPS) y envía una de las operaciones de abajo. Déjala conectada; después ejecuta su mutación correspondiente en otra petición GraphQL HTTP de Bruno. Las mutaciones requieren el token de administrador en **Auth/Headers**. Como las notificaciones son en memoria, ejecuta API y suscripción contra la misma instancia del servidor.

No escribas estas operaciones en una petición `POST` normal: eso solo ejecutaría una vez la operación y no mantendría abierta la conexión para recibir eventos.

#### Al crear un Funko

```graphql
subscription EscucharFunkosCreados {
  onFunkoCreado {
    funkoId
    name
    precio
    stock
    categoriaId
    proveedorId
    createdAt
  }
}
```

Con la suscripción conectada, ejecuta `crearFunko` con datos válidos. El evento solo se publica si el Funko se creó correctamente.

#### Al actualizar un Funko

```graphql
subscription EscucharFunkosActualizados {
  onFunkoActualizado {
    funkoId
    name
    precio
    stock
    categoriaId
    updatedAt
  }
}
```

Con la conexión abierta, ejecuta `actualizarFunko` para un Funko existente. El `input` debe incluir `name`, `precio`, `stock`, `categoriaId` y `proveedorId`; `imagen` puede ser una URL HTTP/HTTPS, una ruta `/uploads/funkos/...` o `null`.

#### Al eliminar un Funko

```graphql
subscription EscucharFunkosEliminados {
  onFunkoEliminado {
    funkoId
    deletedAt
  }
}
```

Con la suscripción abierta, ejecuta `eliminarFunko` con un ID existente que todavía no esté eliminado. La mutación hace una eliminación lógica.

#### Evento de stock bajo

```graphql
subscription EscucharStockBajo {
  onStockBajo {
    funkoId
    name
    stockActual
    stockUmbral
    detectedAt
  }
}
```

Este campo está declarado en el esquema, pero actualmente ninguna parte de la aplicación publica eventos en el topic `onStockBajo`; por eso la suscripción conecta, pero no recibirá notificaciones todavía.

Cada suscripción recibe solo eventos futuros; no devuelve eventos anteriores ni el estado actual. Si Bruno muestra una respuesta HTTP `400` o no conserva abierta la conexión, comprueba que la petición sea de tipo WebSocket y que la URL use `ws://` (no `http://`).

## Variables disponibles

| Operación | Variable(s) |
|---|---|
| `funkoById` | `id: Long!` |
| `funkosPaged` | `page: Int`, `size: Int` |
| `categoriasPaged` | `page: Int`, `size: Int` |
| `categoriaById` | `id: String!` |
| `crearFunko` | `input: CreateFunkoInput!` (`name`, `precio`, `stock`, `categoriaId`, `proveedorId`, `imagen`) |
| `actualizarFunko` | `id: Long!`, `input: UpdateFunkoInput!` (`name`, `precio`, `stock`, `categoriaId`, `proveedorId`, `imagen`) |
| `eliminarFunko` | `id: Long!` |
| `actualizarImagenFunko` | `id: Long!`, `file: Upload!` |
