using CSharpFunctionalExtensions;
using FunkoApiV._2.Entity;
using FunkoApiV._2.Error;
using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
namespace FunkoApiV._2.Repository;

public class ProveedorRepository(AppDbContextMongoDb context, ILogger<ProveedorRepository> log) : IProveedorRepository, IScopedService {
    public async Task<IReadOnlyList<Proveedor>> GetAll() {
        log.LogDebug("Obteniendo todos los proveedores.");
        return await context.Proveedor.Where(p => !p.IsDeleted).ToListAsync();
    }

    public async Task<Result<Proveedor, DomainError>> GetById(string id) {
        log.LogDebug("Obteniendo proveedor con id: {Id} ", id);
        if (!ObjectId.TryParse(id, out _))
            return Result.Failure<Proveedor, DomainError>(
                new ValidationError("El id del proveedor no tiene un formato válido"));

        var funko = await context.Proveedor.FirstOrDefaultAsync(p => p.Id.ToString() == id && !p.IsDeleted);
        return funko is not null 
            ? Result.Success<Proveedor, DomainError>(funko)
            : Result.Failure<Proveedor, DomainError>(NotFound.FromId(id, "Proveedor"));
    }

    public async Task<IReadOnlyList<Proveedor>> GetAllDeleted() {
        log.LogDebug("Obteniendo todos los proveedores eliminados");
        return await context.Proveedor.Where(p => p.IsDeleted == true).ToListAsync();
    }

    public async Task<Result<Proveedor, DomainError>> Add(Proveedor proveedor) {
        log.LogDebug("Obteniendo proveedor con id: {proveedor} ", proveedor);
        var nuevo = proveedor with {
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        await context.Proveedor.AddAsync(nuevo);
        await context.SaveChangesAsync();
        log.LogDebug("Proveedor guardado exitosamente");
        return Result.Success<Proveedor, DomainError>(nuevo);
    }

    public async Task<Result<Proveedor, DomainError>> Update(string id, Proveedor proveedor) {
        log.LogDebug("Actualizando proveedor con id: {id} ", id);
        
        var existente = await context.Proveedor.FirstOrDefaultAsync(p => p.Id.ToString() == id && !p.IsDeleted);
        if (existente is null) 
            return Result.Failure<Proveedor, DomainError>(NotFound.FromId(id, "Proveedor"));
        existente.Nombre = proveedor.Nombre;
        existente.Direccion = proveedor.Direccion;
        existente.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        log.LogDebug("Proveedor actualizado exitosamente");
        return Result.Success<Proveedor, DomainError>(existente);
    }

    public async Task<Result<bool, DomainError>> SoftDelete(string id) {
        log.LogDebug("Eliminando proveedor con id: {id} ", id);
        var proveedor = await context.Proveedor.FirstOrDefaultAsync(p => p.Id.ToString() == id && !p.IsDeleted);
        if (proveedor is null)
            return Result.Failure<bool, DomainError>(NotFound.FromId(id, "Proveedor"));
        proveedor.IsDeleted = true;
        proveedor.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        log.LogDebug("Proveedor eliminado exitosamente");
        return Result.Success<bool, DomainError>(true);
    }

}