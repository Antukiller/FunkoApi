using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FunkoApiV._2.Error;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Service;

public interface IFunkoService {
    Task<IReadOnlyList<Funko>> GetAll();

    Task<Result<Funko, DomainError>> GetById(int id);
    
    Task<Result<Funko, DomainError>> Add(Funko funko);

    Task<Result<Funko, DomainError>> Update(int id, Funko funko);
    
    Task<Result<bool, DomainError>> SoftDelete(int id);
}