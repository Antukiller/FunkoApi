using CSharpFunctionalExtensions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;

namespace FunkoApiV._2.Service;

public interface ICategoriaService {
    Task<PagedResult<CategoriaDto>> GetAll(CategoriaFilterRequest filter);

    Task<Result<CategoriaDto, DomainError>> GetById(string id);

    Task<Result<CategoriaDto, DomainError>> Add(CategoriaRequestDto request);

    Task<Result<CategoriaDto, DomainError>> Update(string id, CategoriaRequestDto request);

    Task<Result<bool, DomainError>> Delete(string id);
}
