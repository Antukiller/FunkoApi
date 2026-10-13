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
    public ActionResult<IReadOnlyList<FunkoResponseDto>> GetAll() {
        return Ok(ser.GetAll().Select(f => f.ToDto()).ToList());
    }
    [HttpGet("{id:int}")]
    public ActionResult<FunkoResponseDto> GetById(int id) {
        var resultado = ser.GetById(id);
        return resultado.IsSuccess
            ? Ok(resultado.Value.ToDto())
            : resultado.Error.TopHttpResult<FunkoResponseDto>();
    }
    [HttpPost]
    public ActionResult<FunkoResponseDto> Create([FromBody] FunkoCreateRequest funko) {
        if (!FunkoMapper.TryParseCategoria(funko.Categoria, out var categoria)) {
            var error = ValidationError.Create("Categoría no válida");
            return error.TopHttpResult<FunkoResponseDto>();
        }
        var result = ser.Add(funko.ToModel(categoria));
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value.ToDto())
            : result.Error.TopHttpResult<FunkoResponseDto>();
    }
    [HttpPut("{id:int}")]
    public ActionResult<FunkoResponseDto> Update([FromBody] FunkoUpdateRequest funko, int id) {
        if (!FunkoMapper.TryParseCategoria(funko.Categoria, out var categoria)) {
            var error = ValidationError.Create("Categoría no válida");
            return error.TopHttpResult<FunkoResponseDto>();
        }
        var act = ser.Update(id, funko.ToModel(categoria));
        return act.IsSuccess
            ? Ok(act.Value.ToDto())
            : act.Error.TopHttpResult<FunkoResponseDto>();
    }

    [HttpDelete("{id:int}")]
    public ActionResult<bool> Delete(int id) {
        var result = ser.SoftDelete(id);
        return result.IsSuccess
            ? NoContent()
            : result.Error.TopHttpResult<bool>();
    }

        
    
}
