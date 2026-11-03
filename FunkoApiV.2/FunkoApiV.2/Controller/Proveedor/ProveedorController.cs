using FunkoApiV._2.Dto.Proveedor;
using FunkoApiV._2.Error;
using FunkoApiV._2.Service;
using Microsoft.AspNetCore.Mvc;

namespace FunkoApiV._2.Controller;

[ApiController]
[Route("api/[controller]")]
public class ProveedorController(IProveedorService ser) : ControllerBase {
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProveedorResponseDto>>> GetAll() {
        var funkos = await ser.GetAll();
        return Ok(funkos.ToList());
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<ProveedorResponseDto>> GetById(string id) {
        var resultado = await ser.GetById(id);
        return resultado.IsSuccess
            ? Ok(resultado.Value)
            : resultado.Error.TopHttpResult<ProveedorResponseDto>();
    }
    [HttpGet("deleted")]
    public async Task<ActionResult<ProveedorResponseDto>> GetDeleted() {
        var get = await ser.GetDeleted();
        return Ok(get.ToList());
    }
    [HttpPost]
    public async Task<ActionResult<ProveedorResponseDto>> Create([FromBody] ProveedorRequestDto proveedor) {
        var result = await ser.Add(proveedor);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.Error.TopHttpResult<ProveedorResponseDto>();
    }
    [HttpPut("{id}")]
    public async Task<ActionResult<ProveedorResponseDto>> Update([FromBody] ProveedorRequestDto proveedor, string id) {
        var act = await ser.Update(id, proveedor);
        return act.IsSuccess
            ? Ok(act.Value)
            : act.Error.TopHttpResult<ProveedorResponseDto>();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(string id) {
        var result = await ser.SoftDelete(id);
        return result.IsSuccess
            ? NoContent()
            : result.Error.TopHttpResult<bool>();
    }
    [HttpPost("{id}/restore")]
    public async Task<ActionResult<bool>>  Restore(string id) {
        var result = await ser.Restore(id);
        if (result.IsSuccess) return Ok(result.Value);
        var err = result.Error;
        return result.Error switch {
            NotFound notFound => NotFound(new { message = notFound.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = err.Message })
        };
    }
}