using CSharpFunctionalExtensions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Error;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Repository;

public interface ICategoriaRepository {
    Task<(IReadOnlyList<Categoria> Items, int Total)> GetPagedAsync(CategoriaFilterRequest filter);

    Task<Result<Categoria, DomainError>> GetById(string id);

    Task<Result<Categoria, DomainError>> CreateAsync(Categoria categoria);

    Task<Result<Categoria, DomainError>> UpdateAsync(string id, Categoria categoria);

    Task<Result<bool, DomainError>> DeleteAsync(string id);
}
