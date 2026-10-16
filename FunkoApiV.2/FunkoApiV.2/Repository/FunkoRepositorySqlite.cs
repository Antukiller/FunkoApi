using CSharpFunctionalExtensions;
using FunkoApiV._2.Entity;
using FunkoApiV._2.Error;
using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Models;
using Microsoft.EntityFrameworkCore;

namespace FunkoApiV._2.Repository;

public class FunkoRepositorySqlite(AppDbContext contextSqlite) : IFunkoRepository, IScopedService {
    
    public async Task<IReadOnlyList<Funko>> GetAll() {
        return await contextSqlite.Funkos
            .AsNoTracking()
            .Include(f => f.Categoria)
            .ToListAsync();
    }

    public async Task<Result<Funko, DomainError>> GetById(int id) {
        var funko = await contextSqlite.Funkos.AsNoTracking().Include(c => c.Categoria).FirstOrDefaultAsync(f => f.Id == id);
        return funko is not null
            ? Result.Success<Funko, DomainError>(funko)
            : Result.Failure<Funko, DomainError>(NotFound.FromId(id, "Funko"));
    }

    public async Task<IReadOnlyList<Funko>> GetAllDeleted() {
        return await contextSqlite.Funkos.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(c => c.Categoria)
            .Where(f => f.IsDeleted)
            .ToListAsync();
    }

    public async Task<Result<Funko, DomainError>> Add(Funko funko) {
        var nuevo = funko with {
            CreatedAt = DateTime.Now,
            IsDeleted = false
        };
        await contextSqlite.Funkos.AddAsync(nuevo);
        await contextSqlite.SaveChangesAsync();
        return Result.Success<Funko, DomainError>(nuevo);
    }

    public async Task<Result<Funko, DomainError>> Update(int id, Funko funko) {
        var existente = await contextSqlite.Funkos
            .Include(f => f.Categoria)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (existente is null)
            return Result.Failure<Funko, DomainError>(NotFound.FromId(id, "Funko"));
        existente.Name = funko.Name;
        existente.Precio = funko.Precio;
        existente.Stock = funko.Stock;
        existente.CategoriaId = funko.CategoriaId;
        existente.UpdatedAt = DateTime.UtcNow;
        await contextSqlite.SaveChangesAsync();
        return Result.Success<Funko, DomainError>(existente);
    }

    public async Task<Result<bool, DomainError>> SoftDelete(int id) {
        var funko = await contextSqlite.Funkos.FindAsync(id);
        if (funko is null) 
            return Result.Failure<bool, DomainError>(NotFound.FromId(id, "Funko"));
        var del = funko with {
            IsDeleted = true
        };
        contextSqlite.Entry(del).State = EntityState.Modified;
        await contextSqlite.SaveChangesAsync();
        return Result.Success<bool, DomainError>(true);
    }
 
    public async Task<Result<bool, DomainError>> Restore(int id) {
        var funko = await contextSqlite.Funkos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id  == id && f.IsDeleted);
        if (funko is null) 
            return Result.Failure<bool, DomainError>(NotFound.FromId(id, "Funko"));
        funko.IsDeleted = false;
        await contextSqlite.SaveChangesAsync();
        return Result.Success<bool, DomainError>(true);    }
}