using CSharpFunctionalExtensions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Entity;
using FunkoApiV._2.Error;
using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Models;
using Microsoft.EntityFrameworkCore;

namespace FunkoApiV._2.Repository;

public class CategoriaRepository(AppDbContext context) : ICategoriaRepository, IScopedService {
    public async Task<(IReadOnlyList<Categoria> Items, int Total)> GetPagedAsync(CategoriaFilterRequest filter) {
        IQueryable<Categoria> query = context.Categorias.AsNoTracking();

        if (filter.IncludeDeleted)
            query = query.IgnoreQueryFilters();

        if (!string.IsNullOrWhiteSpace(filter.Nombre))
            query = query.Where(c => EF.Functions.ILike(c.Nombre, $"%{filter.Nombre}%"));

        var total = await query.CountAsync();
        var items = await ApplySort(query, filter.SortBy, filter.Direction)
            .Skip(filter.Page * filter.Size)
            .Take(filter.Size)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Result<Categoria, DomainError>> GetById(string id) {
        var categoria = await context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        return categoria is not null
            ? Result.Success<Categoria, DomainError>(categoria)
            : Result.Failure<Categoria, DomainError>(NotFound.FromId(id, "Categoria"));
    }

    public async Task<Result<Categoria, DomainError>> CreateAsync(Categoria categoria) {
        var nueva = categoria with {
            Id = YouTubeIdValueGenerator.GenerateId(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
        };
        await context.Categorias.AddAsync(nueva);
        await context.SaveChangesAsync();
        return Result.Success<Categoria, DomainError>(nueva);
    }

    public async Task<Result<Categoria, DomainError>> UpdateAsync(string id, Categoria categoria) {
        var existente = await context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (existente is null)
            return Result.Failure<Categoria, DomainError>(NotFound.FromId(id, "Categoria"));

        existente.Nombre = categoria.Nombre;
        existente.Descripcion = categoria.Descripcion;
        existente.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Result.Success<Categoria, DomainError>(existente);
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(string id) {
        var categoria = await context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (categoria is null)
            return Result.Failure<bool, DomainError>(NotFound.FromId(id, "Categoria"));

        categoria.IsDeleted = true;
        categoria.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Result.Success<bool, DomainError>(true);
    }

    private static IQueryable<Categoria> ApplySort(IQueryable<Categoria> query, string sortBy, string direction) {
        var desc = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sortBy.ToLowerInvariant() switch {
            "nombre" or "name" => desc ? query.OrderByDescending(c => c.Nombre) : query.OrderBy(c => c.Nombre),
            "createdat" or "created_at" => desc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => desc ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id)
        };
    }
}
