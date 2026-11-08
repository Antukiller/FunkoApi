using FunkoApiV._2.Dto.Cassandra;
using FunkoApiV._2.Models.Cassandra;

namespace FunkoApiV._2.Mapper.Cassandra;

public static class CassandraMapper {
    public static FunkosPorProveedorDto ToDto(this FunkosPorProveedor model) => new() {
            ProveedorId = model.ProveedorId,
            FunkoId = model.FunkosId,
            Nombre = model.Nombre,
            Stock = model.Stock,
            Precio = model.Precio,
            ProveedorNombre = model.ProveedorNombre
        };
    public static FunkosPorCategoriaDto ToDto(this FunkosPorCategoria model) =>
        new()
        {
            CategoriaId = model.CategoriaId,
            FunkoId = model.FunkosId,
            Nombre = model.Nombre,
            Stock = model.Stock,
            Precio = model.Precio,
            CategoriaNombre = model.CategoriaNombre
        };
    public static FunkoPorProveedorCategoriaDto ToDto(this FunkoPorProveedorCategoria model) =>
        new()
        {
            ProveedorId = model.ProveedorId,
            CategoriaId = model.CategoriaId,
            FunkoId = model.FunkosId,
            Nombre = model.Nombre,
            ProveedorNombre = model.ProveedorNombre,
            CategoriaNombre = model.CategoriaNombre
        };
}