using CSharpFunctionalExtensions;
using FunkoApiV._2.Dto.Cassandra;
using FunkoApiV._2.Error;

namespace FunkoApiV._2.Service.Cassandra;

public interface ICassandraService {
    Task<IReadOnlyList<FunkosPorCategoriaDto>> GetFunkosPorCategoria(string categoriaId);

    Task<IReadOnlyList<FunkosPorProveedorDto>> GetFunkosPorProveedor(string proveedorId);

    Task<IReadOnlyList<FunkoPorProveedorCategoriaDto>> GetFunkosPorProveedorCategoria(string proveedorId, string categoriaId);
}