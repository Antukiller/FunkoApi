# 📄 doc/12 — Servicios de Dominio

---

## 1. Estructura del service

```csharp
public class FunkoService(
    IFunkoRepository repo,
    IProveedorRepository repoProveedor,
    ICategoriaRepository repoCategoria,
    ILogger<FunkoService> log,
    IValidator<FunkoCreateRequest> createValidation,
    IValidator<FunkoUpdateRequest> updateValidation,
    IStorageService storage
) : IFunkoService
{
    public async Task<Result<FunkoResponseDto, DomainError>> Add(FunkoCreateRequest request)
    {
        log.LogInformation("Creando funko: {@Funko}", request);

        // 1. Validar existencia de relaciones (categoría y proveedor)
        var categoria = await repoCategoria.GetById(request.CategoriaId);
        if (categoria.IsFailure)
            return Result.Failure<FunkoResponseDto, DomainError>(
                new Conflict("La categoría especificada no existe"));

        var proveedor = await repoProveedor.GetById(request.ProveedorId);
        if (proveedor.IsFailure)
            return Result.Failure<FunkoResponseDto, DomainError>(
                new Conflict("El proveedor especificado no existe"));

        // 2. Validar DTO con FluentValidation
        var validation = await createValidation.ValidateAsync(request);
        if (!validation.IsValid)
            return Result.Failure<FunkoResponseDto, DomainError>(
                new ValidationError(validation.Errors[0].ErrorMessage));

        // 3. Persistir
        return (await repo.CreateAsync(request.ToModel())).Map(f => f.ToDto());
    }
}
```

---

## 2. Reglas de los services

| Regla | Detalle |
|-------|---------|
| Dueño de la regla de negocio | Nunca en el controller |
| Devuelve `Result<T, DomainError>` | Nunca excepciones de negocio |
| Loguea | `LogInformation` al entrar, `LogWarning` en no fatales |
| Mapea entidad → DTO | El repository devuelve entidades, el service DTOs |
| No conoce `HttpContext` | Solo lógica pura de negocio |
| Efectos secundarios en `Tap` | Email, websocket, señalR, caché |

---

## 3. Servicio con caché (decorador)

```csharp
public class CachedFunkoService(IFunkoService inner, ICacheService cache) : IFunkoService
{
    // Delegar al inner y gestionar caché
}
```

---

## 4. Reglas de negocio específicas

| Regla | Implementación |
|-------|---------------|
| No borrar categoría con funkos | Consultar `funkoRepo.GetPagedAsync` por `CategoriaId`, si `Total > 0` → `Conflict` |
| Imagen relativa | Validar que la ruta empiece con `/uploads/` y no sea URL absoluta |
| ID categoría YouTube | 11 caracteres, generado automáticamente en `CreateAsync` |
| ID proveedor ObjectId | 24 chars hex, validado por regex |