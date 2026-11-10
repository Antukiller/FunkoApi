using CSharpFunctionalExtensions;
using FunkoApiV._2.Error;
using FunkoApiV._2.Models.Cassandra;

namespace FunkoApiV._2.Repository.Cassandra;

public interface ICassandraRepository {
    Task<IReadOnlyList<FunkosPorCategoria>> GetFunkosPorCategoria(string categoriaId);
    Task<IReadOnlyList<FunkosPorProveedor>> GetFunkosPorProveedor(string proveedorId);
    Task<IReadOnlyList<FunkoPorProveedorCategoria>> GetFunkosPorProveedorCategoria(string proveedorId, string categoriaId);

    Task SincronizarFunkoAsync(long funkoId, string nombre, int stock, decimal precio,
        string categoriaId, string categoriaNombre,
        string proveedorId, string proveedorNombre);

    Task EliminarFunkoAsync(string categoriaId, string proveedorId, long funkoId);
}