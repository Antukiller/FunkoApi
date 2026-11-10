using Cassandra;
using Cassandra.Data.Linq;
using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Models.Cassandra;
using ISession = Cassandra.ISession;

namespace FunkoApiV._2.Repository.Cassandra;

public class CassandraRepository(ISession session): ICassandraRepository, IScopedService {
    private readonly Table<FunkosPorCategoria> _funkosPorCategori = new(session);
    private readonly Table<FunkosPorProveedor> _funkosPorProveedor = new(session);
    private readonly Table<FunkoPorProveedorCategoria> _funkosPorProveedorCategoria = new(session);
     
    public async Task<IReadOnlyList<FunkosPorCategoria>> GetFunkosPorCategoria(string categoriaId) {
        var funkos = await _funkosPorCategori
            .Where(f => f.CategoriaId == categoriaId)
            .ExecuteAsync();
        return funkos.ToList();
    }

    public async Task<IReadOnlyList<FunkosPorProveedor>> GetFunkosPorProveedor(string proveedorId) {
        var funkos = await  _funkosPorProveedor
            .Where(f => f.ProveedorId == proveedorId)
            .ExecuteAsync();
        return funkos.ToList();
    }

    public async Task<IReadOnlyList<FunkoPorProveedorCategoria>> GetFunkosPorProveedorCategoria(string proveedorId, string categoriaId) {
        var funkos = await _funkosPorProveedorCategoria
            .Where(f => f.CategoriaId == categoriaId && f.ProveedorId == proveedorId)
            .ExecuteAsync();
        return funkos.ToList();
    }

    public async Task SincronizarFunkoAsync(long funkoId, string nombre, int stock, decimal precio, string categoriaId,
        string categoriaNombre, string proveedorId, string proveedorNombre) {
        await session.ExecuteAsync(new SimpleStatement(
            "INSERT INTO funkos_por_categorias " +
            "(categoria_id, funkos_id, nombre, stock, precio, categoria_nombre) " +
            "VALUES (?, ?, ?, ?, ?, ?)",
            categoriaId, funkoId, nombre, stock, precio, categoriaNombre));

        await session.ExecuteAsync(new SimpleStatement(
            "INSERT INTO funkos_por_proveedor " +
            "(proveedor_id, funkos_id, nombre, stock, precio, proveedor_nombre) " +
            "VALUES (?, ?, ?, ?, ?, ?)",
            proveedorId, funkoId, nombre, stock, precio, proveedorNombre));

        await session.ExecuteAsync(new SimpleStatement(
            "INSERT INTO funkos_por_proveedor_categoria " +
            "(proveedor_id, categoria_id, funkos_id, nombre, proveedor_nombre, categoria_nombre) " +
            "VALUES (?, ?, ?, ?, ?, ?)",
            proveedorId, categoriaId, funkoId, nombre, proveedorNombre, categoriaNombre));   
    }

    public async Task EliminarFunkoAsync(string categoriaId, string proveedorId, long funkoId) {
        await session.ExecuteAsync(new SimpleStatement(
            "DELETE FROM funkos_por_categorias WHERE categoria_id = ? AND funkos_id = ?",
            categoriaId, funkoId));

        await session.ExecuteAsync(new SimpleStatement(
            "DELETE FROM funkos_por_proveedor WHERE proveedor_id = ? AND funkos_id = ?",
            proveedorId, funkoId));

        await session.ExecuteAsync(new SimpleStatement(
            "DELETE FROM funkos_por_proveedor_categoria " +
            "WHERE proveedor_id = ? AND categoria_id = ? AND funkos_id = ?",
            proveedorId, categoriaId, funkoId));
    }
}