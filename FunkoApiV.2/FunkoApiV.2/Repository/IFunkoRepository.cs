using CSharpFunctionalExtensions;
using FunkoApiV._2.Error;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Repository;

public interface IFunkoRepository {
    IReadOnlyList<Funko> GetAll();

    Result<Funko, DomainError> GetById(int id);
    
    Result<Funko, DomainError> Add(Funko funko);

    Result<Funko, DomainError> Update(int id, Funko funko);
    
    Result<bool, DomainError> SoftDelete(int id);
}