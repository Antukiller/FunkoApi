using FunkoApiV._2.Dto;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Mapper;

public static class FunkoMapper {
    public static FunkoResponseDto ToDto(this Funko f) => new() {
        Id = f.Id,
        Name = f.Name,
        Precio = f.Precio,
        Stock = f.Stock,
        Categoria = f.Categoria.ToString(),
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt
    };

    public static Funko ToModel(this FunkoCreateRequest dto, Categoria categoria) => new() {
        Name = dto.Name,
        Precio = dto.Precio,
        Stock = dto.Stock,
        Categoria = categoria,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IsDeleted = false
    };
    
    public static Funko ToModel(this FunkoUpdateRequest dto, Categoria categoria) => new() {
        Name = dto.Name,
        Precio = dto.Precio,
        Stock = dto.Stock,
        Categoria = categoria
    };
    public static bool TryParseCategoria(string valor, out Categoria categoria) {
        return Enum.TryParse(valor, true, out categoria)
               && Enum.IsDefined(typeof(Categoria), categoria);
    }
}