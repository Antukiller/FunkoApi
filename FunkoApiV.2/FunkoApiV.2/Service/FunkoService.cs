using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FunkoApiV._2.Error;
using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Models;
using FunkoApiV._2.Repository;

namespace FunkoApiV._2.Service;

public class FunkoService(IFunkoRepository repo) : IFunkoService, IScopedService {
    public IReadOnlyList<Funko> GetAll() => repo.GetAll();
    public Result<Funko, DomainError> GetById(int id) => repo.GetById(id);
    public Result<Funko, DomainError> Add(Funko funko) => repo.Add(funko);
    public Result<Funko, DomainError> Update(int id, Funko funko) => repo.Update(id, funko);
    public Result<bool, DomainError> SoftDelete(int id) => repo.SoftDelete(id);
}