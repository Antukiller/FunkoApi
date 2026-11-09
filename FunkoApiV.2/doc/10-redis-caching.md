# 📄 doc/10 — Caché Redis

---

## 1. Patrón Cache-Aside

```
Request → Service → ¿Caché hit? → Devolver caché
                          ↓ no
                          Repositorio → BD → Guardar en caché → Devolver
```

---

## 2. Implementación

```csharp
public class CachedFunkoService(IFunkoService inner, ICacheService cache) : IFunkoService
{
    private static string ByIdKey(int id) => $"funko:{id}";

    public async Task<Result<FunkoResponseDto, DomainError>> GetById(int id)
    {
        var key = ByIdKey(id);
        var cached = await cache.GetAsync<FunkoResponseDto>(key);
        if (cached is not null)
            return Result.Success<FunkoResponseDto, DomainError>(cached);

        var result = await inner.GetById(id);
        if (result.IsSuccess)
            await cache.SetAsync(key, result.Value);
        return result;
    }

    public async Task<Result<FunkoResponseDto, DomainError>> Update(int id, FunkoUpdateRequest request)
    {
        var result = await inner.Update(id, request);
        if (result.IsSuccess)
            await cache.RemoveAsync(ByIdKey(id)); // Invalidar
        return result;
    }

    public async Task<Result<bool, DomainError>> Delete(int id)
    {
        var result = await inner.Delete(id);
        if (result.IsSuccess)
            await cache.RemoveAsync(ByIdKey(id)); // Invalidar
        return result;
    }
}
```

---

## 3. Claves de caché

| Clave | Contenido | TTL |
|-------|-----------|-----|
| `funko:{id}` | FunkoResponseDto | Configurable (ej. 10 min) |
| `categoria:{id}` | CategoriaDto | Configurable |
| `categorias:all` | Lista paginada | No se cachea (filtros variables) |

> 📌 Los listados paginados **no se cachean** porque cada consulta con filtros distintos es única.

---

## 4. Invalidación

- **Create**: no invalida (nuevo registro)
- **Update**: invalida `funko:{id}`
- **Delete**: invalida `funko:{id}`

---

## 5. Configuración Redis

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = FunkoConfig.RedisConnection);

builder.Services.AddSingleton<ICacheService, RedisService>();
```

---

## 6. Servicio de caché

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}
```