using CSharpFunctionalExtensions;
using FunkoApiMongo.Cache;
using FunkoApiV._2.Dto.Proveedor;
using FunkoApiV._2.Error;

namespace FunkoApiV._2.Service.Proveedor;

public class CachedProveedorService(IProveedorService inner, ICacheService cache) : IProveedorService {
    private const string AllKey = "proveedor:all";
    private const string DeletedKey = "proveedor:deleted";
    private static string ByIdKey(string id) => $"proveedor:{id}";
    
    public async Task<IReadOnlyList<ProveedorResponseDto>> GetAll() {
        var result = await cache.GetOrSetAsync(
            AllKey,
            async () => await inner.GetAll()
        );
        return result ?? [];
    }

    public async Task<IReadOnlyCollection<ProveedorResponseDto>> GetDeleted() {
        var result = await cache.GetOrSetAsync(
            DeletedKey,
            async () => await inner.GetDeleted()
        );
        return result ?? [];
    }

    public async Task<Result<ProveedorResponseDto, DomainError>> GetById(string id) {
        var key = ByIdKey(id);
        var cached = await cache.GetAsync<ProveedorResponseDto>(key);
        if (cached != null) 
            return Result.Success<ProveedorResponseDto, DomainError>(cached);
        var result = await inner.GetById(id);
        if (result.IsSuccess)
            await cache.SetAsync(key, result.Value);
        return result;
    }

    public async Task<Result<ProveedorResponseDto, DomainError>> Add(ProveedorRequestDto proveedor) {
        var res = await inner.Add(proveedor);
        if (res.IsSuccess) {
            await cache.RemoveAsync(AllKey);
        }
        return res;
    }

    public async Task<Result<ProveedorResponseDto, DomainError>> Update(string id, ProveedorRequestDto funko) {
        var res = await inner.Update(id, funko);
        if (!res.IsSuccess) return res;
        await cache.RemoveAsync(ByIdKey(id));
        await cache.RemoveAsync(AllKey);
        return res;
    }

    public async Task<Result<bool, DomainError>> SoftDelete(string id) {
        var res = await inner.SoftDelete(id);
        if (!res.IsSuccess) return res;
        await cache.RemoveAsync(ByIdKey(id));
        await cache.RemoveAsync(AllKey);
        await cache.RemoveAsync(DeletedKey);
        return res;
        
    }
}