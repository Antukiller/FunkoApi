using FunkoApiV._2.Dto.Cassandra;
using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Mapper.Cassandra;
using FunkoApiV._2.Repository.Cassandra;

namespace FunkoApiV._2.Service.Cassandra;

public class CassandraService(ICassandraRepository cassandraRepository) : ICassandraService, IScopedService {
    public async Task<IReadOnlyList<FunkosPorCategoriaDto>> GetFunkosPorCategoria(string categoriaId) {
        var funkoList = await cassandraRepository.GetFunkosPorCategoria(categoriaId);
        return funkoList.Select(p => p.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<FunkosPorProveedorDto>> GetFunkosPorProveedor(string proveedorId) {
        var funkoList = await cassandraRepository.GetFunkosPorProveedor(proveedorId);
        return funkoList.Select(p => p.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<FunkoPorProveedorCategoriaDto>> GetFunkosPorProveedorCategoria(string proveedorId, string categoriaId) {
        var funkoList = await cassandraRepository.GetFunkosPorProveedorCategoria(proveedorId, categoriaId);
        return funkoList.Select(p => p.ToDto()).ToList();
    }
}