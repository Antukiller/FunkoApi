using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Mapper;
using FunkoApiV._2.Repository;
using HotChocolate;

namespace FunkoApiV._2.GraphQL.Queries;

public class FunkoQuery {
    public async Task<IReadOnlyList<FunkoDto>> GetFunkos(
        [Service] IFunkoRepository funkoRepository) {
        var funkos = await funkoRepository.GetAllAsync();
        return funkos.Select(funko => funko.ToFunkoDto()).ToList();
    }

    public async Task<FunkoDto?> GetFunkoById(
        long id,
        [Service] IFunkoRepository funkoRepository) {
        var result = await funkoRepository.GetByIdAsync(id);
        return result.IsSuccess ? result.Value.ToFunkoDto() : null;
    }

    public async Task<PagedResult<FunkoDto>> GetFunkosPaged(
        [Service] IFunkoRepository funkoRepository,
        int page = 1,
        int size = 10) {
        page = Math.Max(page, 1);
        size = Math.Clamp(size, 1, 100);
        var filter = new FunkoFilterRequest {
            Page = page - 1,
            Size = size
        };
        var result = await funkoRepository.GetPagedAsync(filter);

        return new PagedResult<FunkoDto> {
            Items = result.Items.Select(funko => funko.ToFunkoDto()).ToList(),
            TotalCount = result.Total,
            Page = page,
            PageSize = size
        };
    }

    public async Task<PagedResult<CategoriaDto>> GetCategoriasPaged(
        [Service] ICategoriaRepository categoriaRepository,
        int page = 1,
        int size = 10) {
        page = Math.Max(page, 1);
        size = Math.Clamp(size, 1, 100);
        var filter = new CategoriaFilterRequest {
            Page = page - 1,
            Size = size
        };
        var result = await categoriaRepository.GetPagedAsync(filter);

        return new PagedResult<CategoriaDto> {
            Items = result.Items.Select(categoria => categoria.ToDto()).ToList(),
            TotalCount = result.Total,
            Page = page,
            PageSize = size
        };
    }

    public async Task<CategoriaDto?> GetCategoriaById(
        string id,
        [Service] ICategoriaRepository categoriaRepository) {
        var result = await categoriaRepository.GetById(id);
        return result.IsSuccess ? result.Value.ToDto() : null;
    }
}
