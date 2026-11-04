using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;
using FunkoApiV._2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunkoApiV._2.Controller;

[ApiController]
[Route("api/categorias")]
public class CategoriaController(ICategoriaService ser) : ControllerBase {
    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoriaDto>>> GetAll([FromQuery] CategoriaFilterRequest filter) =>
        Ok(await ser.GetAll(filter));
    [HttpGet("{id:length(11)}")]
    public async Task<ActionResult<CategoriaDto>> GetById(string id) {
        var result = await ser.GetById(id);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<CategoriaDto>();
    }
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Create([FromBody] CategoriaRequestDto categoria) {
        var result = await ser.Add(categoria);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.Error.TopHttpResult<CategoriaDto>();
    }
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id:length(11)}")]
    public async Task<ActionResult<CategoriaDto>> Update(string id, [FromBody] CategoriaRequestDto categoria) {
        var result = await ser.Update(id, categoria);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<CategoriaDto>();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:length(11)}")]
    public async Task<ActionResult<bool>> Delete(string id) {
        var result = await ser.Delete(id);
        return result.IsSuccess
            ? NoContent()
            : result.Error.TopHttpResult<bool>();
    }
}
