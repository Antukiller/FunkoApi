# 📄 doc/13 — Borrado Lógico y Reglas de Negocio

---

## 1. Problema

Al eliminar una categoría, no se pueden borrar los funkos asociados porque perderían su clasificación. Se utiliza **borrado lógico** (`IsDeleted`).

---

## 2. Implementación en Entidades

```csharp
public class Categoria {
    public string Id { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public DateTime? UpdatedAt { get; set; }
}

public class Funko {
    public int Id { get; set; }
    public string CategoriaId { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
}
```

---

## 3. Filtro Global en DbContext

```csharp
modelBuilder.Entity<Categoria>().HasQueryFilter(c => !c.IsDeleted);
modelBuilder.Entity<Funko>().HasQueryFilter(f => !f.IsDeleted);
```

Esto excluye automáticamente los borrados de todas las consultas LINQ.

---

## 4. Flujo de Eliminación de Categoría

```
1. Recibir DELETE /api/categorias/{id}
2. Consultar si hay funkos con CategoriaId = id
3. Si Total > 0 → devolver 409 Conflict
   - Mensaje: "No se puede borrar la categoría si hay un funko asociado"
4. Si Total == 0 → marcar IsDeleted = true en la categoría
   - devolver 204 No Content
```

---

## 5. Código del Service

```csharp
public async Task<Result<bool, DomainError>> Delete(string id)
{
    // Verificar funkos asociados
    var paged = await funkoRepo.GetPagedAsync(new FunkoFilterRequest {
        CategoriaId = id,
        Page = 0,
        Size = 1
    });
    if (paged.Total > 0)
        return Result.Failure<bool, DomainError>(
            new Conflict("No se puede borrar la categoría si hay un funko asociado"));

    return await repo.DeleteAsync(id);
}
```

---

## 6. Flujo de Eliminación de Funko

```
1. Recibir DELETE /api/funkos/{id}
2. Buscar funko por id
3. Si no existe → 404 NotFound
4. Marcar IsDeleted = true, UpdatedAt = ahora
5. Guardar cambios → 204 No Content
```

---

## 7. Consultar borrados

Para ver entidades borradas (administración):

```csharp
// Ignorar el filtro global
var borrados = await context.Funkos
    .IgnoreQueryFilters()
    .Where(f => f.IsDeleted)
    .ToListAsync();
```

O usar filtro `IncludeDeleted = true` en el repository.