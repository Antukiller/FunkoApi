using System;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Mapper;

public static class FunkoMapper {
    public static FunkoDto ToFunkoDto(this Funko f) => new(
        f.Id,
        f.Name,
        f.Precio,
        f.Stock,
        f.Imagen,
        f.Categoria?.Nombre ?? string.Empty,
        f.CategoriaId,
        f.ProveedorId,
        f.CreatedAt,
        f.UpdatedAt
    );

    public static FunkoResponseDto ToDto(this Funko f) => new(
        f.Id,
        f.Name,
        f.Precio,
        f.Stock,
        f.Imagen,
        f.Categoria?.Nombre ?? string.Empty,
        f.CategoriaId,
        f.ProveedorId,
        f.CreatedAt,
        f.UpdatedAt
    );

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