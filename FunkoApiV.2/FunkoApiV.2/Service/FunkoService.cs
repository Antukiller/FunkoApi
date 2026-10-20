using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Error;
using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Mapper;
using FunkoApiV._2.Models;
using FunkoApiV._2.Repository;

namespace FunkoApiV._2.Service;

public class FunkoService(IFunkoRepository repo, ICategoriaRepository cateRepo) : IFunkoService, IScopedService {
    public async Task<IReadOnlyList<FunkoResponseDto>> GetAll() => (await repo.GetAll()).Select(p => p.ToDto()).ToList();
    public async Task<IReadOnlyCollection<FunkoResponseDto>> GetDeleted() => (await repo.GetAllDeleted()).Select(p => p.ToDto()).ToList();
    public async Task<Result<FunkoResponseDto, DomainError>> GetById(int id) => (await repo.GetById(id)).Map(p => p.ToDto());
    public async Task<Result<FunkoResponseDto, DomainError>> Add(FunkoCreateRequest funko) => (await repo.Add(funko.ToModel())).Map(p => p.ToDto());
    public async Task<Result<FunkoResponseDto, DomainError>> Update(int id, FunkoUpdateRequest funko) => (await repo.Update(id, funko.ToModel())).Map(p => p.ToDto());
    public async Task<Result<bool, DomainError>> SoftDelete(int id) => await repo.SoftDelete(id);
    public async Task<Result<bool, DomainError>> Restore(int id) => await repo.Restore(id);
    public async Task<IReadOnlyList<CategoriaDto>> GetAllCategories() => (await cateRepo.GetAll()).Select(p => p.ToDto()).ToList();
    public async Task<Result<CategoriaDto, DomainError>> GetByIdCategories(int id) => (await cateRepo.GetById(id)).Map(p => p.ToDto());
}