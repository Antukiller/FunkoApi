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
}