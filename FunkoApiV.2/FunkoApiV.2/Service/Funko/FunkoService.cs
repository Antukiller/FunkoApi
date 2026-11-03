using CSharpFunctionalExtensions;
using FluentValidation;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;
using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Mapper;
using FunkoApiV._2.Repository;
using FunkoApiV._2.Storage;

namespace FunkoApiV._2.Service;

public class FunkoService(
    IFunkoRepository repo,
    ILogger<FunkoService> log,
    IValidator<FunkoCreateRequest> createValidation,
    IValidator<FunkoUpdateRequest> updateValidation,
    IFileStorageService storage
) : IFunkoService, IScopedService {

    public async Task<PagedResult<FunkoResponseDto>> GetAll(FunkoFilterRequest filter) {
        log.LogInformation("Buscando funkos con filtros {@Filter}", filter);
        var (items, total) = await repo.GetPagedAsync(filter);
        return new PagedResult<FunkoResponseDto> {
            Items = items.Select(f => f.ToDto()).ToList(),
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.Size
        };
    }

    public async Task<Result<FunkoResponseDto, DomainError>> GetById(int id) {
        log.LogInformation("Obteniendo funko por id: {@Id}", id);
        return (await repo.GetByIdAsync(id)).Map(f => f.ToDto());
    }

    public async Task<Result<FunkoResponseDto, DomainError>> Add(FunkoCreateRequest request) {
        log.LogInformation("Creando funko: {@Funko}", request);
        var validation = await createValidation.ValidateAsync(request);
        if (!validation.IsValid) {
            log.LogWarning("Validación fallida al crear funko: {@Errors}", validation.Errors);
            return Result.Failure<FunkoResponseDto, DomainError>(
                new ValidationError(validation.Errors[0].ErrorMessage));
        }

        return (await repo.CreateAsync(request.ToModel())).Map(f => f.ToDto());
    }

    public async Task<Result<FunkoResponseDto, DomainError>> Update(int id, FunkoUpdateRequest request) {
        log.LogInformation("Actualizando funko {Id}: {@Funko}", id, request);
        var validation = await updateValidation.ValidateAsync(request);
        if (!validation.IsValid) {
            log.LogWarning("Validación fallida al actualizar funko {Id}: {@Errors}", id, validation.Errors);
            return Result.Failure<FunkoResponseDto, DomainError>(
                new ValidationError(validation.Errors[0].ErrorMessage));
        }

        return (await repo.UpdateAsync(id, request.ToModel())).Map(f => f.ToDto());
    }

    public async Task<Result<bool, DomainError>> Delete(int id) {
        log.LogInformation("Eliminando funko: {@Id}", id);
        return await repo.DeleteAsync(id);
    }

    public async Task<Result<string, DomainError>> UploadImagen(IFormFile file) {
        try {
            return Result.Success<string, DomainError>(await storage.SaveAsync(file, "funkos"));
        } catch (ArgumentException ex) {
            log.LogWarning(ex, "Fichero de imagen no válido");
            return Result.Failure<string, DomainError>(new ValidationError(ex.Message));
        }
    }

    public async Task<Result<FunkoResponseDto, DomainError>> UpdateImagen(int id, IFormFile file) {
        var funko = await repo.GetByIdAsync(id);
        if (funko.IsFailure)
            return Result.Failure<FunkoResponseDto, DomainError>(funko.Error);

        Result<string, DomainError> guardada;
        try {
            guardada = await storage.SaveAsync(file, "funkos");
        } catch (ArgumentException ex) {
            log.LogWarning(ex, "Fichero de imagen no válido para el funko {Id}", id);
            return Result.Failure<FunkoResponseDto, DomainError>(new ValidationError(ex.Message));
        }

        var actualizado = await repo.SetImagenAsync(id, guardada.Value);
        if (actualizado.IsFailure) {
            storage.Delete(guardada.Value);
            return Result.Failure<FunkoResponseDto, DomainError>(actualizado.Error);
        }

        storage.Delete(funko.Value.Imagen);
        return actualizado.Map(f => f.ToDto());
    }
}
