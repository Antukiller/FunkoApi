using CSharpFunctionalExtensions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Proveedor;
using FunkoApiV._2.Error;

namespace FunkoApiV._2.Service;

public interface IProveedorService {
    Task<IReadOnlyList<ProveedorResponseDto>> GetAll();
    Task<IReadOnlyCollection<ProveedorResponseDto>> GetDeleted();
    Task<Result<ProveedorResponseDto, DomainError>> GetById(string id);
    Task<Result<ProveedorResponseDto, DomainError>> Add(ProveedorRequestDto proveedor);
    Task<Result<ProveedorResponseDto, DomainError>> Update(string id, ProveedorRequestDto proveedor);
    Task<Result<bool, DomainError>> SoftDelete(string id);
}