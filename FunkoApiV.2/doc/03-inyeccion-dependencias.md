# 📄 doc/03 — Inyección de Dependencias

---

## 1. Patrón utilizado

Se usan **extension methods** en `Infrastructures/*Config.cs`, uno por aspecto:

| Archivo | Responsabilidad |
|---------|-----------------|
| `DatabaseConfig.cs` | DbContext (PostgreSQL + MongoDB) |
| `ServicesConfig.cs` | Servicios de negocio |
| `RepositoriesConfig.cs` | Repositorios |
| `CacheConfig.cs` | Redis + ICacheService |
| `AuthConfig.cs` | JWT + políticas de autorización |
| `SwaggerConfig.cs` | Swagger/OpenAPI |
| `CorsConfig.cs` | CORS |

---

## 2. Ejemplo: DatabaseConfig

```csharp
public static class DatabaseConfig
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgresConnection = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Cadena de conexión no configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(postgresConnection));

        services.AddDbContext<AppDbContextMongoDb>(options =>
            options.UseMongoDB(configuration["Mongo:Connection"], configuration["Mongo:Database"]));

        return services;
    }
}
```

---

## 3. Registro de servicios

```csharp
services.AddScoped<IFunkoService, FunkoService>();
services.AddScoped<ICategoriaService, CategoriaService>();
services.AddScoped<IFunkoRepository, FunkoRepository>();
services.AddScoped<ICategoriaRepository, CategoriaRepository>();
services.AddSingleton<ICacheService, RedisService>();
```

- `Scoped`: servicios y repositorios (vida por request HTTP)
- `Singleton`: caché Redis (vida de la aplicación)

---

## 4. Scrutor (opcional)

El proyecto usa Scrutor para registro automático de algunas dependencias:

```csharp
builder.Services.AddDependencies();
```

> ⚠️ Se prefiere registro explícito sobre escaneo por reflexión para mayor claridad y rendimiento.