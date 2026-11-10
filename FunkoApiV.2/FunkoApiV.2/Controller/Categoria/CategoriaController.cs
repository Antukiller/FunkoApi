using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;
using FunkoApiV._2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunkoApiV._2.Controller;

[ApiController]
[Route("api/categorias")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
public class CategoriaController(ICategoriaService ser) : ControllerBase {
    /// <summary>Lista categorías paginadas, con filtro por nombre y opciones de ordenación.</summary>
    /// <param name="filter">Filtro opcional por nombre; página base cero, tamaño 1–100 y orden.</param>
    /// <response code="200">Página de categorías y total coincidente.</response>
    /// <response code="400">Algún parámetro de filtro o paginación no es válido.</response>
    [ProducesResponseType(typeof(PagedResult<CategoriaDto>), StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoriaDto>>> GetAll([FromQuery] CategoriaFilterRequest filter) =>
        Ok(await ser.GetAll(filter));

    /// <summary>Obtiene una categoría por su identificador de 11 caracteres.</summary>
    /// <param name="id">Identificador de la categoría.</param>
    /// <response code="200">Categoría encontrada.</response>
    /// <response code="404">No existe una categoría activa con ese identificador.</response>
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [HttpGet("{id:length(11)}")]
    public async Task<ActionResult<CategoriaDto>> GetById(string id) {
        var result = await ser.GetById(id);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<CategoriaDto>();
    }

    /// <summary>Crea una categoría.</summary>
    /// <param name="categoria">Nombre requerido y descripción opcional.</param>
    /// <response code="201">Categoría creada.</response>
    /// <response code="400">El nombre o la descripción no cumplen sus restricciones.</response>
    /// <response code="401">Falta autenticación.</response>
    /// <response code="403">El usuario no tiene el rol ADMIN.</response>
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Create([FromBody] CategoriaRequestDto categoria) {
        var result = await ser.Add(categoria);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.Error.TopHttpResult<CategoriaDto>();
    }

    /// <summary>Actualiza el nombre y la descripción de una categoría.</summary>
    /// <param name="id">Identificador de 11 caracteres.</param>
    /// <param name="categoria">Nombre requerido y descripción opcional.</param>
    /// <response code="200">Categoría actualizada.</response>
    /// <response code="400">El identificador o los datos no son válidos.</response>
    /// <response code="404">La categoría no existe o está eliminada.</response>
    /// <response code="401">Falta autenticación.</response>
    /// <response code="403">El usuario no tiene el rol ADMIN.</response>
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id:length(11)}")]
    public async Task<ActionResult<CategoriaDto>> Update(string id, [FromBody] CategoriaRequestDto categoria) {
        var result = await ser.Update(id, categoria);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<CategoriaDto>();
    }

    /// <summary>Elimina lógicamente una categoría.</summary>
    /// <param name="id">Identificador de 11 caracteres.</param>
    /// <response code="204">Categoría marcada como eliminada; no devuelve cuerpo.</response>
    /// <response code="404">La categoría no existe o está eliminada.</response>
    /// <response code="401">Falta autenticación.</response>
    /// <response code="403">El usuario no tiene el rol ADMIN.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:length(11)}")]
    public async Task<ActionResult<bool>> Delete(string id) {
        var result = await ser.Delete(id);
        return result.IsSuccess
            ? NoContent()
            : result.Error.TopHttpResult<bool>();
    }
}
