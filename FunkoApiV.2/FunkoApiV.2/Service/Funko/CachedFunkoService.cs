using CSharpFunctionalExtensions;
using FunkoApiMongo.Cache;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;

namespace FunkoApiV._2.Service;

public class CachedFunkoService(IFunkoService inner, ICacheService cache) : IFunkoService {
    private static string ByIdKey(long id) => $"funko:{id}";

    /// <summary>
    /// Los listados van paginados y filtrados, así que cada consulta es distinta:
    /// no se cachean. Solo se cachea la ficha por id.
    /// </summary>
    public Task<PagedResult<FunkoResponseDto>> GetAll(FunkoFilterRequest filter) =>
        inner.GetAll(filter);

    public async Task<Result<FunkoResponseDto, DomainError>> GetById(long id) {
        var key = ByIdKey(id);
        var cached = await cache.GetAsync<FunkoResponseDto>(key);
        if (cached is not null)
            return Result.Success<FunkoResponseDto, DomainError>(cached);

        var result = await inner.GetById(id);
        if (result.IsSuccess)
            await cache.SetAsync(key, result.Value);
        return result;
    }

    public Task<Result<FunkoResponseDto, DomainError>> Add(FunkoCreateRequest request) =>
        inner.Add(request);

    public async Task<Result<FunkoResponseDto, DomainError>> Update(long id, FunkoUpdateRequest request) {
        var result = await inner.Update(id, request);
        if (result.IsSuccess)
            await cache.RemoveAsync(ByIdKey(id));
        return result;
    }

    public async Task<Result<bool, DomainError>> Delete(long id) {
        var result = await inner.Delete(id);
        if (result.IsSuccess)
            await cache.RemoveAsync(ByIdKey(id));
        return result;
    }

    public Task<Result<string, DomainError>> UploadImagen(IFormFile file) =>
        inner.UploadImagen(file);

    public async Task<Result<FunkoResponseDto, DomainError>> UpdateImagen(long id, IFormFile file) {
        var result = await inner.UpdateImagen(id, file);
        if (result.IsSuccess)
            await cache.RemoveAsync(ByIdKey(id));
        return result;
    }
}
