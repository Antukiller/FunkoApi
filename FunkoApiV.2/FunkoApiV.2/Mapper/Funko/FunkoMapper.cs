using System;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Mapper;

public static class FunkoMapper {
    public static FunkoResponseDto ToDto(this Funko f) => new() {
        Id = f.Id,
        Name = f.Name,
        Precio = f.Precio,
        Stock = f.Stock,
        Imagen = f.Imagen,
        Categoria = f.Categoria?.Nombre ?? string.Empty,
        CategoriaId = f.CategoriaId,
        ProveedorId = f.ProveedorId,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt
    };

    public static Funko ToModel(this FunkoCreateRequest dto) => new() {
        Name = dto.Name,
        Precio = dto.Precio,
        Stock = dto.Stock,
        Imagen =  dto.Imagen,
        CategoriaId = dto.CategoriaId,
        ProveedorId =  dto.ProveedorId,
        CreatedAt = DateTime.UtcNow,
    };

    public static Funko ToModel(this FunkoUpdateRequest dto) => new() {
        Name = dto.Name,
        Precio = dto.Precio,
        Stock = dto.Stock,
        Imagen = dto.Imagen,
        CategoriaId = dto.CategoriaId,
        ProveedorId = dto.ProveedorId,
        UpdatedAt = DateTime.UtcNow
    };
    
}