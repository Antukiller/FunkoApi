# 📄 doc/06 — Mejores Prácticas REST

---

## 1. Recursos y URLs

| Recurso | URL base | ID |
|---------|----------|-----|
| Funko | `/api/funkos` | `int` (auto) |
| Categoría | `/api/categorias` | `string` (YouTube 11 chars) |
| Proveedor | `/api/Proveedor` | `string` (ObjectId 24 chars) |

> 📌 No se usa versionado `api/v1` hardcodeado en la ruta.

---

## 2. Métodos HTTP

| Método | Uso | Cuerpo request | Cuerpo response |
|--------|-----|----------------|-----------------|
| GET | Leer (listar/obtener) | Ninguno | DTO o lista paginada |
| POST | Crear | RequestDto | ResponseDto + 201 |
| PUT | Actualizar completo | RequestDto | ResponseDto + 200 |
| DELETE | Borrar lógico | Ninguno | 204 No Content |
| PATCH | Actualizar parcial (imagen) | Form-data | ResponseDto + 200 |

---

## 3. Paginación

```csharp
public record FunkoFilterRequest {
    public string? Nombre { get; init; }
    public string? CategoriaId { get; init; }
    public string? ProveedorId { get; init; }
    public decimal? PrecioMin { get; init; }
    public decimal? PrecioMax { get; init; }
    public int Page { get; init; } = 0;
    public int Size { get; init; } = 10;
}
```

**Respuesta paginada:**
```csharp
public record PagedResult<T> {
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => Page + 1 < TotalPages;
    public bool HasPreviousPage => Page > 0;
}
```

> 🛡️ **Clamp defensivo**: `Math.Clamp(filter.Size, 1, 100)` para evitar abusos del cliente.

---

## 4. Filtros dinámicos

```csharp
query = query.Include(f => f.Categoria);

if (!string.IsNullOrWhiteSpace(filter.Nombre))
    query = query.Where(f => EF.Functions.ILike(f.Name, $"%{filter.Nombre}%"));

if (!string.IsNullOrWhiteSpace(filter.CategoriaId))
    query = query.Where(f => f.CategoriaId == filter.CategoriaId);
```

---

## 5. Soft Delete

- Las entidades tienen propiedad `IsDeleted` (bool)
- Filtro global en `DbContext`: `HasQueryFilter(f => !f.IsDeleted)`
- `DELETE` no elimina físicamente: marca `IsDeleted = true`
- Para ver borrados: `IgnoreQueryFilters()` + `IncludeDeleted = true`

---

## 6. Concurrencia optimista

Entidad con `byte[] RowVersion` (PostgreSQL `bytea`) para detectar conflictos de actualización simultánea.

---

## 7. Imágenes

- En BD solo se guarda la **ruta relativa** (ej. `/uploads/funkos/placeholder.png`)
- Nunca se guarda el archivo binario en BD
- Las imágenes reales se almacenan en sistema de archivos o almacenamiento externo