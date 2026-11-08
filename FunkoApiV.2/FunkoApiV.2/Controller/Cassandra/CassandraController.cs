using FunkoApiV._2.Dto.Cassandra;
using FunkoApiV._2.Service.Cassandra;
using Microsoft.AspNetCore.Mvc;

namespace FunkoApiV._2.Controller.Cassandra;


[ApiController]
[Route("api/cassandra")]
public class CassandraController(ICassandraService cassandraService) : ControllerBase
{
    [HttpGet("categoria/{categoriaId}")]
    public async Task<ActionResult<IReadOnlyList<FunkosPorCategoriaDto>>> GetFunkosPorCategoria(string categoriaId)
    {
        var funkos = await cassandraService.GetFunkosPorCategoria(categoriaId);
        return Ok(funkos);
    }

    [HttpGet("proveedor/{proveedorId}")]
    public async Task<ActionResult<IReadOnlyList<FunkosPorProveedorDto>>> GetFunkosPorProveedor(string proveedorId)
    {
        var funkos = await cassandraService.GetFunkosPorProveedor(proveedorId);
        return Ok(funkos);
    }

    [HttpGet("proveedor/{proveedorId}/categoria/{categoriaId}")]
    public async Task<ActionResult<IReadOnlyList<FunkoPorProveedorCategoriaDto>>> GetFunkosPorProveedorCategoria(string proveedorId, string categoriaId)
    {
        var funkos = await cassandraService.GetFunkosPorProveedorCategoria(proveedorId, categoriaId);
        return Ok(funkos);
    }
}