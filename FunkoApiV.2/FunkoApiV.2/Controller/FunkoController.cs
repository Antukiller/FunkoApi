using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Error;
using FunkoApiV._2.Mapper;
using FunkoApiV._2.Models;
using FunkoApiV._2.Repository;
using FunkoApiV._2.Service;
using Microsoft.AspNetCore.Mvc;

namespace FunkoApiV._2.Controller;

[ApiController]
[Route("api/[controller]")]
public class FunkoController(IFunkoService ser) : ControllerBase {
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FunkoResponseDto>>> GetAll() {
        var funkos = await ser.GetAll();
        return Ok(funkos.Select(f => f.ToDto()).ToList());
        
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FunkoResponseDto>> GetById(int id) {
        var resultado = await ser.GetById(id);
        return resultado.IsSuccess
            ? Ok(resultado.Value.ToDto())
            : resultado.Error.TopHttpResult<FunkoResponseDto>();
    }
    [HttpPost]
    public async Task<ActionResult<FunkoResponseDto>> Create([FromBody] FunkoCreateRequest funko) {
        var result = await ser.Add(funko.ToModel());
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value.ToDto())
            : result.Error.TopHttpResult<FunkoResponseDto>();
    }
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FunkoResponseDto>> Update([FromBody] FunkoUpdateRequest funko, int id) {
        var act = await ser.Update(id, funko.ToModel());
        return act.IsSuccess
            ? Ok(act.Value.ToDto())
            : act.Error.TopHttpResult<FunkoResponseDto>();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<bool>> Delete(int id) {
        var result = await ser.SoftDelete(id);
        return result.IsSuccess
            ? NoContent()
            : result.Error.TopHttpResult<bool>();
    }

        
    
}
