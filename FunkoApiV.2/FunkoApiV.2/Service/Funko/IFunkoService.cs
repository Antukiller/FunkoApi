using CSharpFunctionalExtensions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;

namespace FunkoApiV._2.Service;

public interface IFunkoService {
    Task<PagedResult<FunkoResponseDto>> GetAll(FunkoFilterRequest filter);

    Task<Result<FunkoResponseDto, DomainError>> GetById(int id);

    Task<Result<FunkoResponseDto, DomainError>> Add(FunkoCreateRequest request);

    Task<Result<FunkoResponseDto, DomainError>> Update(int id, FunkoUpdateRequest request);

    Task<Result<bool, DomainError>> Delete(int id);

    Task<Result<string, DomainError>> UploadImagen(IFormFile file);

    Task<Result<FunkoResponseDto, DomainError>> UpdateImagen(int id, IFormFile file);
}
