using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;
using FunkoApiV._2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunkoApiV._2.Controller;

[ApiController]
[Route("api/funkos")]
public class FunkoController(IFunkoService ser) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<PagedResult<FunkoResponseDto>>> GetAll([FromQuery] FunkoFilterRequest filter) =>
        Ok(await ser.GetAll(filter));
    [Authorize(Roles = "ADMIN")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FunkoResponseDto>> GetById(long id) {
        var resultado = await ser.GetById(id);
        return resultado.IsSuccess
            ? Ok(resultado.Value)
            : resultado.Error.TopHttpResult<FunkoResponseDto>();
    }
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<ActionResult<FunkoResponseDto>> Create([FromBody] FunkoCreateRequest funko) {
        var result = await ser.Add(funko);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.Error.TopHttpResult<FunkoResponseDto>();
    }
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FunkoResponseDto>> Update(long id, [FromBody] FunkoUpdateRequest funko) {
        var result = await ser.Update(id, funko);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<FunkoResponseDto>();
    }
    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<bool>> Delete(long id) {
        var result = await ser.Delete(id);
        return result.IsSuccess
            ? NoContent()
            : result.Error.TopHttpResult<bool>();
    }
    [HttpPost("imagen")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImagenResponseDto>> UploadImagen([FromForm] IFormFile file) {
        var result = await ser.UploadImagen(file);
        return result.IsSuccess
            ? Ok(new ImagenResponseDto(result.Value))
            : result.Error.TopHttpResult<ImagenResponseDto>();
    }

    [HttpPatch("{id:int}/imagen")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FunkoResponseDto>> UpdateImagen(long id, [FromForm] IFormFile file) {
        var result = await ser.UpdateImagen(id, file);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<FunkoResponseDto>();
    }
}
