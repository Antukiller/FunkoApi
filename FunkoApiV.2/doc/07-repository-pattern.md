# 📄 doc/07 — Patrón Repositorio

---

## 1. Estructura del repositorio

```csharp
public interface IFunkoRepository
{
    Task<(IReadOnlyList<Funko> Items, int Total)> GetPagedAsync(FunkoFilterRequest filter);
    Task<Result<Funko, DomainError>> GetByIdAsync(int id);
    Task<Result<Funko, DomainError>> CreateAsync(Funko funko);
    Task<Result<Funko, DomainError>> UpdateAsync(int id, Funko funko);
    Task<Result<bool, DomainError>> DeleteAsync(int id);
    Task<Result<bool, DomainError>> SetImagenAsync(int id, string imagen);
}
```

---

## 2. Reglas del repositorio

| Regla | Detalle |
|-------|---------|
| `AsNoTracking()` | En todas las consultas de solo lectura |
| `Include(...)` | Para relaciones necesarias (evita lazy loading) |
| No lanza excepciones | Devuelve `null` o `Result.Failure`, el service decide |
| Soft delete | `IsDeleted = true` en vez de `Remove()` |
| Filtro global | `HasQueryFilter` en DbContext excluye borrados |
| Paginación con techo | `Math.Clamp(filter.Size, 1, 100)` |

---

## 3. Implementación EF Core (PostgreSQL)

```csharp
public class FunkoRepository(AppDbContext context) : IFunkoRepository
{
    public async Task<Result<Funko, DomainError>> GetByIdAsync(int id)
    {
        var funko = await context.Funkos
            .AsNoTracking()
            .Include(f => f.Categoria)
            .FirstOrDefaultAsync(f => f.Id == id);
        return funko is not null
            ? Result.Success<Funko, DomainError>(funko)
            : Result.Failure<Funko, DomainError>(NotFound.FromId(id.ToString(), "Funko"));
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(int id)
    {
        var funko = await context.Funkos.FirstOrDefaultAsync(f => f.Id == id);
        if (funko is null)
            return Result.Failure<bool, DomainError>(NotFound.FromId(id.ToString(), "Funko"));
        funko.IsDeleted = true;
        funko.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Result.Success<bool, DomainError>(true);
    }
}
```

---

## 4. Implementación MongoDB (Proveedor — responsabilidad del compañero)

El repositorio de proveedores usa `MongoDB.EntityFrameworkCore` o `MongoDB.Driver` directamente sobre la colección `proveedor`.

---

## 5. Interfaz vs Implementación

- La interfaz define el contrato (`IFunkoRepository`)
- La implementación depende de `DbContext` o `IMongoCollection`
- El service depende de la interfaz, nunca de la implementación concreta