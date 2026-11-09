# 📄 doc/05 — Validación en Cascada

---

## 1. FluentValidation

Se usa **FluentValidation** para validar los DTOs de entrada. Un validator por operación:

```
CreateFunkoValidator  →  Valida FunkoCreateRequest
UpdateFunkoValidator  →  Valida FunkoUpdateRequest
CreateCategoriaValidator → Valida CategoriaRequestDto
```

---

## 2. Ejemplo: FunkoCreateValidator

```csharp
public class FunkoCreateValidator : AbstractValidator<FunkoCreateRequest>
{
    public FunkoCreateValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(20).WithMessage("El nombre no puede exceder 20 caracteres");

        RuleFor(p => p.Precio)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0")
            .LessThanOrEqualTo(50).WithMessage("El precio no puede exceder 50");

        RuleFor(p => p.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");

        RuleFor(p => p.CategoriaId)
            .NotEmpty().WithMessage("La categoría es obligatoria")
            .Length(11).WithMessage("El ID de categoría debe tener 11 caracteres");

        RuleFor(p => p.ProveedorId)
            .NotEmpty().WithMessage("El proveedor es obligatorio")
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("ID de proveedor inválido (ObjectId)");

        RuleFor(p => p.Imagen)
            .NotEmpty().WithMessage("La imagen es obligatoria")
            .MaximumLength(255).WithMessage("La ruta de imagen es demasiado larga");
    }
}
```

---

## 3. Validación en la capa de Service

El service valida antes de persistir:

```csharp
var validation = await createValidation.ValidateAsync(request);
if (!validation.IsValid)
{
    var errors = validation.Errors
        .GroupBy(e => e.PropertyName)
        .ToDictionary(
            g => g.Key,
            g => g.Select(e => e.ErrorMessage).ToArray()
        );
    return Result.Failure<FunkoResponseDto, DomainError>(
        new ValidationError("Errores de validación", errors)
    );
}
```

---

## 4. Cascada de validación

1. **DTO**: Anotaciones `[Required]`, `[MaxLength]`, etc. (validación básica del modelo)
2. **FluentValidation**: Reglas de negocio complejas (validación en el service)
3. **Repository**: Restricciones de base de datos (longitud, tipos, foreign keys)
4. **Service**: Reglas de dominio (borrado lógico, existencia de relaciones)

> 📌 Los mensajes de validación están **en español**.