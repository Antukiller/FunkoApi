# 📄 doc/14 — Mapeadores

---

## 1. Patrón

Mapeadores como `static class` con extension methods. El mapeo manual es la fuente de verdad; AutoMapper se usa solo como complemento.

```csharp
public static class FunkoMapper
{
    public static FunkoResponseDto ToDto(this Funko funko) =>
        new(
            funko.Id,
            funko.Name,
            funko.Precio,
            funko.Stock,
            funko.Imagen,
            funko.Categoria.Nombre ?? string.Empty,
            funko.CategoriaId,
            funko.ProveedorId,
            funko.CreatedAt,
            funko.UpdatedAt
        );

    public static Funko ToEntity(this FunkoCreateRequest dto) => new()
    {
        Name = dto.Name,
        Precio = dto.Precio,
        Stock = dto.Stock,
        Imagen = dto.Imagen,
        CategoriaId = dto.CategoriaId,
        ProveedorId = dto.ProveedorId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = null,
        IsDeleted = false
    };
}
```

---

## 2. Reglas

| Regla | Detalle |
|-------|---------|
| `static class` | Por agregado (`FunkoMapper`, `CategoriaMapper`) |
| Extension methods | `ToDto()`, `ToEntity()`, `ToDtoList()` |
| Null-conditional | `funko.Categoria?.Nombre ?? string.Empty` |
| No usar AutoMapper como sustituto | Solo para casos simples, el manual es la verdad |
| Fechas UTC | `DateTime.UtcNow` para CreatedAt/UpdatedAt |

---

## 3. Mapeo con relaciones

```csharp
public static CategoriaDto ToDto(this Categoria categoria) =>
    new(
        categoria.Id,
        categoria.Nombre,
        categoria.Descripcion,
        categoria.CreatedAt,
        categoria.UpdatedAt
    );
```

> 📌 Cuando una entidad tiene navegación a otra (Funko → Categoria), el mapper manual resuelve el nombre relacionado sin cargar entidades adicionales innecesariamente.