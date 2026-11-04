using CSharpFunctionalExtensions;
using FunkoApiV._2.Error;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Repository;

public interface IProveedorRepository {
    Task<IReadOnlyList<Proveedor>> GetAll();

    Task<Result<Proveedor, DomainError>> GetById(string id);

    Task<IReadOnlyList<Proveedor>> GetAllDeleted();
    
    Task<Result<Proveedor, DomainError>> Add(Proveedor proveedor);

    Task<Result<Proveedor, DomainError>> Update(string id, Proveedor proveedor);
    
    Task<Result<bool, DomainError>> SoftDelete(string id);
    
}