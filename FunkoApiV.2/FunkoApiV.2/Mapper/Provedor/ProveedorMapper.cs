using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Proveedor;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Mapper.Provedor;

public static class ProveedorMapper {
    public static ProveedorResponseDto ToDto(this Proveedor f) => new() {
        Id = f.Id.ToString(),
        Nombre = f.Nombre,
        Direccion =  f.Direccion,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt
    };

    public static Proveedor ToModel(this ProveedorRequestDto dto) => new() {
        Nombre = dto.Nombre,
        Direccion =  dto.Direccion,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = null,
        IsDeleted = false
    };

}