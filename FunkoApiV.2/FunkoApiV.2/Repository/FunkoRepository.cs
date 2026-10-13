using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using FunkoApiV._2.Error;
using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Repository;

public class FunkoRepository : IFunkoRepository, ISingletonService {
    public FunkoRepository() {
        Add(new Funko {
            Name = "Goku Super Saiyan",
            Categoria = Categoria.DragonBall,
            Precio = 15.99,
            Stock = true
        });

        Add(new Funko {
            Name = "Spider-Man No Way Home",
            Categoria = Categoria.Marvel,
            Precio = 16.50,
            Stock = true
        });

        Add(new Funko {
            Name = "Naruto Modo Sabio",
            Categoria = Categoria.Naruto,
            Precio = 18.00,
            Stock = false
        });

        Add(new Funko {
            Name = "Iron Man Mark LXXXV",
            Categoria = Categoria.Marvel,
            Precio = 22.99,
            Stock = true
        });
    }
    private readonly Dictionary<int, Funko> _funkos = [];
    private int _nextId = 1;

    public IReadOnlyList<Funko> GetAll() => Activos();
    

    public Result<Funko, DomainError> GetById(int id) {
        var funko = _funkos.TryGetValue(id, out var f) && !f.IsDeleted ? f : null;
        return funko is not null
            ? Result.Success<Funko, DomainError>(funko)
            : Result.Failure<Funko, DomainError>(NotFound.FromId(id, "Funko"));
    }
    public Result<Funko, DomainError> Add(Funko funko) {
        var nuevo = funko with {
            Id = _nextId++,
            CreatedAt = DateTime.Now
        };
        _funkos[nuevo.Id] = nuevo;
        return Result.Success<Funko, DomainError>(nuevo);
    }

    public Result<Funko, DomainError> Update(int id, Funko funko) {
        if (!_funkos.TryGetValue(id, out var f) || f.IsDeleted)
            return Result.Failure<Funko, DomainError>(NotFound.FromId(id, "Funko"));
        var actualizado = f with {
            Name = funko.Name,
            Categoria = funko.Categoria,
            Precio = funko.Precio,
            Stock = funko.Stock,
            UpdatedAt = DateTime.UtcNow
        };
        _funkos[id] = actualizado;
        return Result.Success<Funko, DomainError>(actualizado);
    }
    public Result<bool, DomainError> SoftDelete(int id) {
        if (!_funkos.TryGetValue(id, out var f) || f.IsDeleted)
            return Result.Failure<bool, DomainError>(NotFound.FromId(id, "Funko"));
        if (f.IsDeleted)
            return false;
        _funkos[id] = f with { IsDeleted = true };
        return true;
    }
    private List<Funko> Activos() => [.. _funkos.Values.Where(p => !p.IsDeleted)];
}