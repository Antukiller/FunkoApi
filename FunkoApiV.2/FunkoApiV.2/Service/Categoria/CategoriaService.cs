using CSharpFunctionalExtensions;
using FluentValidation;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;
using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Mapper;
using FunkoApiV._2.Repository;

namespace FunkoApiV._2.Service;

public class CategoriaService(
    ICategoriaRepository repo,
    IFunkoRepository funkoRepo,
    ILogger<CategoriaService> log,
    IValidator<CategoriaRequestDto> validation
) : ICategoriaService, IScopedService {

    public async Task<PagedResult<CategoriaDto>> GetAll(CategoriaFilterRequest filter) {
        log.LogInformation("Buscando categorías con filtros {@Filter}", filter);
        var (items, total) = await repo.GetPagedAsync(filter);
        return new PagedResult<CategoriaDto> {
            Items = items.Select(c => c.ToDto()).ToList(),
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.Size
        };
    }

    public async Task<Result<CategoriaDto, DomainError>> GetById(string id) {
        log.LogInformation("Obteniendo categoría por id: {@Id}", id);
        return (await repo.GetById(id)).Map(c => c.ToDto());
    }

    public async Task<Result<CategoriaDto, DomainError>> Add(CategoriaRequestDto request) {
        log.LogInformation("Creando categoría: {@Categoria}", request);
        var result = await validation.ValidateAsync(request);
        if (!result.IsValid) {
            log.LogWarning("Validación fallida al crear categoría: {@Errors}", result.Errors);
            return Result.Failure<CategoriaDto, DomainError>(
                new ValidationError(result.Errors[0].ErrorMessage));
        }

        return (await repo.CreateAsync(request.ToModel())).Map(c => c.ToDto());
    }

    public async Task<Result<CategoriaDto, DomainError>> Update(string id, CategoriaRequestDto request) {
        log.LogInformation("Actualizando categoría {Id}: {@Categoria}", id, request);
        var result = await validation.ValidateAsync(request);
        if (!result.IsValid) {
            log.LogWarning("Validación fallida al actualizar categoría {Id}: {@Errors}", id, result.Errors);
            return Result.Failure<CategoriaDto, DomainError>(
                new ValidationError(result.Errors[0].ErrorMessage));
        }

        return (await repo.UpdateAsync(id, request.ToModel())).Map(c => c.ToDto());
    }

    public async Task<Result<bool, DomainError>> Delete(string id) {
        log.LogInformation("Eliminando categoría: {@Id}", id);
        var paged = await funkoRepo.GetPagedAsync(new FunkoFilterRequest { CategoriaId = id, Page = 0, Size = 1 });
        if (paged.Total > 0) 
            return Result.Failure<bool, DomainError>(
                new Conflict("No se puede borrar la categoria si hay un funko asociado"));
        return await repo.DeleteAsync(id);
    }
}
