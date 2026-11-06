using CSharpFunctionalExtensions;
using FunkoApiV._2.Error;
using FunkoApiV._2.Models.Cassandra;

namespace FunkoApiV._2.Repository.Cassandra;

public interface ICassandraRepository {
    Task<IReadOnlyList<FunkosPorCategoria>> GetFunkosPorCategoria(string categoriaId);
    Task<IReadOnlyList<FunkosPorProveedor>> GetFunkosPorProveedor(string proveedorId);
    Task<IReadOnlyList<FunkoPorProveedorCategoria>> GetFunkosPorProveedorCategoria(string proveedorId, string categoriaId);
}