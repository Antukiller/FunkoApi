using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FunkoApiV._2.Error;
using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Models;
using FunkoApiV._2.Repository;

namespace FunkoApiV._2.Service;

public class FunkoService(IFunkoRepository repo) : IFunkoService, IScopedService {
    public Task<IReadOnlyList<Funko>> GetAll() => repo.GetAll();
    public Task<Result<Funko, DomainError>> GetById(int id) => repo.GetById(id);
    public Task<Result<Funko, DomainError>> Add(Funko funko) => repo.Add(funko);
    public Task<Result<Funko, DomainError>> Update(int id, Funko funko) => repo.Update(id, funko);
    public Task<Result<bool, DomainError>> SoftDelete(int id) => repo.SoftDelete(id);
}