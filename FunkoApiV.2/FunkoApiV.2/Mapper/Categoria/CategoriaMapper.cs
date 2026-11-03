using FunkoApiV._2.Dto;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Mapper;

public static class CategoriaMapper {
    public static CategoriaDto ToDto(this Categoria c) => new(
        c.Id,
        c.Nombre,
        c.Descripcion,
        c.CreatedAt,
        c.UpdatedAt
    );

    public static Categoria ToModel(this CategoriaRequestDto dto) => new() {
        Nombre = dto.Nombre,
        Descripcion = dto.Descripcion,
        CreatedAt = DateTime.UtcNow,
        IsDeleted = false
    };
}
