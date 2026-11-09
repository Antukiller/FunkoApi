# 📄 doc/02 — Arquitectura y Pipeline HTTP

---

## 1. Arquitectura por capas (Onion-like híbrida)

```
HTTP Request
   ↓
Middleware (GlobalExceptionHandler, SecurityHeaders)
   ↓
Controller  ──→  DTO de entrada + Validator
   ↓
Service     ──→  Result<T, DomainError>  ← reglas de negocio, caché
   ↓
Repository  ──→  EF Core / MongoDB / Redis
   ↓
DbContext
```

### Reglas de dependencia

| Capa | Puede depender de | NO puede depender de |
|------|-------------------|----------------------|
| Controller | Service, DTO | Repository, DbContext |
| Service | Repository, DomainError, DTO, Cache | Controller, HttpContext |
| Repository | DbContext, DomainError | Service, Controller |
| Models (entidades) | Nada | Todo el resto |

> 📌 El controller **nunca** habla con el repositorio. El service **nunca** devuelve entidad EF al controller: siempre DTO.

---

## 2. Pipeline HTTP (`Program.cs`)

El orden del middleware es crítico:

```csharp
// 1. Servicios (vía extension methods en Infrastructures/)
builder.Services.AddSerilogConfiguration();
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddRepositoriesConfiguration();
builder.Services.AddServicesConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddAuthenticationConfiguration(builder.Configuration);

// 2. Pipeline de middleware (orden = prioridad)
app.UseGlobalExceptionHandler();      // Captura excepciones no controladas
app.UseSecurityHeaders();             // Headers de seguridad HTTP
app.UseRateLimiting();                // Límite por IP/endpoint
app.UseHsts();                        // Solo producción
app.UseHttpsRedirection();            // Solo producción
app.UseCorsPolicy();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 3. Migraciones y seed
await app.ApplyDatabaseMigrationsAsync();
await app.SeedDatabaseAsync();
```

---

## 3. Registro de DI — Extension methods

Cada fichero `Infrastructures/*Config.cs` tiene una responsabilidad:

```csharp
// Infrastructures/ServicesConfig.cs
public static class ServicesConfig
{
    public static IServiceCollection AddServicesConfiguration(
        this IServiceCollection services)
    {
        services.AddScoped<IFunkoService, FunkoService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        return services;
    }
}
```

> ❌ No se usa el patrón `IScopedService`/`ISingletonService` con escaneo por reflexión. Se usan extension methods explícitos.

---

## 4. `public partial class Program`

Obligatorio para tests de integración con `WebApplicationFactory<Program>`:

```csharp
public partial class Program;
```

---

## 5. Estilo de código

- **Namespace file-scoped**: `namespace FunkoApiV._2.Service;`
- **Constructor primario**: `public class FunkoService(IFunkoRepository repo, ...) : IFunkoService`
- **Llave de apertura**: siempre en línea nueva
- **`var`**: solo cuando el tipo es evidente
- **Métodos async**: sufijo `Async` (`FindByIdAsync`, `GetAllAsync`)
- **XML docs**: obligatorio en todo miembro público (`///`)