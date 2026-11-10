using FunkoApiV._2.Dto.Cassandra;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Service.Cassandra;
using Microsoft.AspNetCore.Mvc;

namespace FunkoApiV._2.Controller.Cassandra;


[ApiController]
[Route("api/cassandra")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
public class CassandraController(ICassandraService cassandraService) : ControllerBase
{
    /// <summary>Consulta en Cassandra los Funkos asociados a una categoría.</summary>
    /// <param name="categoriaId">Identificador de categoría.</param>
    /// <response code="200">Lista de Funkos; puede estar vacía.</response>
    [HttpGet("categoria/{categoriaId}")]
    [ProducesResponseType(typeof(IReadOnlyList<FunkosPorCategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FunkosPorCategoriaDto>>> GetFunkosPorCategoria(string categoriaId)
    {
        var funkos = await cassandraService.GetFunkosPorCategoria(categoriaId);
        return Ok(funkos);
    }

    /// <summary>Consulta en Cassandra los Funkos asociados a un proveedor.</summary>
    /// <param name="proveedorId">ObjectId del proveedor.</param>
    /// <response code="200">Lista de Funkos; puede estar vacía.</response>
    [HttpGet("proveedor/{proveedorId}")]
    [ProducesResponseType(typeof(IReadOnlyList<FunkosPorProveedorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FunkosPorProveedorDto>>> GetFunkosPorProveedor(string proveedorId)
    {
        var funkos = await cassandraService.GetFunkosPorProveedor(proveedorId);
        return Ok(funkos);
    }

    /// <summary>Consulta en Cassandra los Funkos de un proveedor y una categoría.</summary>
    /// <param name="proveedorId">ObjectId del proveedor.</param>
    /// <param name="categoriaId">Identificador de categoría.</param>
    /// <response code="200">Lista de Funkos; puede estar vacía.</response>
    [HttpGet("proveedor/{proveedorId}/categoria/{categoriaId}")]
    [ProducesResponseType(typeof(IReadOnlyList<FunkoPorProveedorCategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FunkoPorProveedorCategoriaDto>>> GetFunkosPorProveedorCategoria(string proveedorId, string categoriaId)
    {
        var funkos = await cassandraService.GetFunkosPorProveedorCategoria(proveedorId, categoriaId);
        return Ok(funkos);
    }
}