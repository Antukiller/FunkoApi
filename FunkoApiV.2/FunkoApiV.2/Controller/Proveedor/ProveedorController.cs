using FunkoApiV._2.Dto.Proveedor;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;
using FunkoApiV._2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunkoApiV._2.Controller;

[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
public class ProveedorController(IProveedorService ser) : ControllerBase {
    /// <summary>Lista proveedores activos. Requiere rol ADMIN.</summary>
    /// <response code="200">Lista de proveedores activos; puede estar vacía.</response>
    /// <response code="401">Falta autenticación.</response>
    /// <response code="403">El usuario no tiene el rol ADMIN.</response>
    [ProducesResponseType(typeof(IReadOnlyList<ProveedorResponseDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProveedorResponseDto>>> GetAll() {
        var funkos = await ser.GetAll();
        return Ok(funkos.ToList());
    }

    /// <summary>Obtiene un proveedor por su ObjectId de MongoDB. Requiere rol ADMIN.</summary>
    /// <param name="id">ObjectId hexadecimal de 24 caracteres.</param>
    /// <response code="200">Proveedor encontrado.</response>
    /// <response code="400">El identificador no es un ObjectId válido.</response>
    /// <response code="404">No existe un proveedor activo con ese identificador.</response>
    /// <response code="401">Falta autenticación.</response>
    /// <response code="403">El usuario no tiene el rol ADMIN.</response>
    [ProducesResponseType(typeof(ProveedorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "ADMIN")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProveedorResponseDto>> GetById(string id) {
        var resultado = await ser.GetById(id);
        return resultado.IsSuccess
            ? Ok(resultado.Value)
            : resultado.Error.TopHttpResult<ProveedorResponseDto>();
    }

    /// <summary>Lista proveedores eliminados lógicamente. Requiere rol ADMIN.</summary>
    /// <response code="200">Lista de proveedores eliminados; puede estar vacía.</response>
    /// <response code="401">Falta autenticación.</response>
    /// <response code="403">El usuario no tiene el rol ADMIN.</response>
    [ProducesResponseType(typeof(IReadOnlyList<ProveedorResponseDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = "ADMIN")]
    [HttpGet("deleted")]
    public async Task<ActionResult<IReadOnlyList<ProveedorResponseDto>>> GetDeleted() {
        var get = await ser.GetDeleted();
        return Ok(get.ToList());
    }

    /// <summary>Crea un proveedor. Requiere rol ADMIN.</summary>
    /// <param name="proveedor">Nombre (2–20 caracteres) y dirección (2–200 caracteres).</param>
    /// <response code="201">Proveedor creado.</response>
    /// <response code="400">Nombre o dirección no válidos.</response>
    /// <response code="401">Falta autenticación.</response>
    /// <response code="403">El usuario no tiene el rol ADMIN.</response>
    [ProducesResponseType(typeof(ProveedorResponseDto), StatusCodes.Status201Created)]
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<ActionResult<ProveedorResponseDto>> Create([FromBody] ProveedorRequestDto proveedor) {
        var result = await ser.Add(proveedor);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.Error.TopHttpResult<ProveedorResponseDto>();
    }

    /// <summary>Actualiza un proveedor activo. Requiere rol ADMIN.</summary>
    /// <param name="id">ObjectId hexadecimal de 24 caracteres.</param>
    /// <param name="proveedor">Nuevo nombre y dirección.</param>
    /// <response code="200">Proveedor actualizado.</response>
    /// <response code="400">El identificador o los datos no son válidos.</response>
    /// <response code="404">El proveedor no existe o está eliminado.</response>
    /// <response code="401">Falta autenticación.</response>
    /// <response code="403">El usuario no tiene el rol ADMIN.</response>
    [ProducesResponseType(typeof(ProveedorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ProveedorResponseDto>> Update([FromBody] ProveedorRequestDto proveedor, string id) {
        var act = await ser.Update(id, proveedor);
        return act.IsSuccess
            ? Ok(act.Value)
            : act.Error.TopHttpResult<ProveedorResponseDto>();
    }

    /// <summary>Elimina lógicamente un proveedor. Requiere rol ADMIN.</summary>
    /// <param name="id">ObjectId hexadecimal de 24 caracteres.</param>
    /// <response code="204">Proveedor marcado como eliminado; no devuelve cuerpo.</response>
    /// <response code="400">El identificador no es válido.</response>
    /// <response code="404">El proveedor no existe o ya está eliminado.</response>
    /// <response code="401">Falta autenticación.</response>
    /// <response code="403">El usuario no tiene el rol ADMIN.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(string id) {
        var result = await ser.SoftDelete(id);
        return result.IsSuccess
            ? NoContent()
            : result.Error.TopHttpResult<bool>();
    }
}