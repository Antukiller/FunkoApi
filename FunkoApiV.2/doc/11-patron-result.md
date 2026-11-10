# 📄 doc/11 — Patrón Result (ROP)

---

## 1. ¿Qué es?

**Result Pattern** (Railway Oriented Programming) usando `CSharpFunctionalExtensions`:

```csharp
Task<Result<T, DomainError>>      // Con valor de retorno
Task<UnitResult<DomainError>>     // Sin valor de retorno
```

Los services **no lanzan excepciones** para errores de negocio, devuelven `Result.Failure`.

---

## 2. Firma de servicios

```csharp
// Con valor
Task<Result<FunkoResponseDto, DomainError>> GetByIdAsync(int id);
Task<Result<FunkoResponseDto, DomainError>> Add(FunkoCreateRequest request);

// Sin valor
Task<UnitResult<DomainError>> DeleteAsync(int id);
```

---

## 3. Construcción de resultados

```csharp
// Éxito
return Result.Success<FunkoResponseDto, DomainError>(dto);

// Fallo
return Result.Failure<FunkoResponseDto, DomainError>(
    FunkoError.NotFound(id)
);

// Efectos secundarios tras éxito
return Result.Success(dto)
    .Tap(_ => logger.LogInformation("Creado: {Id}", dto.Id))
    .Tap(_ => cache.RemoveAsync(key));
```

---

## 4. Jerarquía de errores

```csharp
public abstract record DomainError(string Message);

public sealed record NotFound(string Message) : DomainError(Message);
public sealed record ValidationError(string Message) : DomainError(Message);
public sealed record Conflict(string Message) : DomainError(Message);
public sealed record BusinessRuleError(string Message) : DomainError(Message);
public sealed record InternalError(string Message) : DomainError(Message);
```

---

## 5. Fábrica de errores por agregado

```csharp
// Errors/Funko/FunkoError.cs
public static class FunkoError
{
    public static NotFound NotFound(int id) =>
        NotFoundError.FromId(id, "Funko");
    
    public static Conflict CategoriaNoExiste() =>
        new("La categoría especificada no existe");
}
```

---

## 6. Traducción a HTTP

Extensión centralizada `DomainErrorExtensions.TopHttpResult<T>()`:

| Error de dominio | HTTP |
|------------------|------|
| NotFound | 404 |
| ValidationError | 400 |
| Conflict | 409 |
| BusinessRuleError | 422 |
| UnauthorizedError | 401 |
| Default | 500 |