using FunkoApiV._2.Error;
using FunkoApiV._2.Models;
using FunkoApiV._2.Repository;
using Microsoft.AspNetCore.Mvc;

namespace FunkoApiV._2.Controller;

[ApiController]
[Route("api/[controller]")]
public class FunkoController(IFunkoRepository repo) : ControllerBase {
    
    private static List<Funko> _funkos= new();

    [HttpGet]
    public ActionResult<List<Funko>> GetAll() {
        return Ok(repo.GetAll());
    }
    [HttpGet("{id:int}")]
    public ActionResult<Funko> GetById(int id) {
        var resultado = repo.GetById(id);
        return resultado.IsSuccess
            ? Ok(resultado)
            : resultado.Error.TopHttpResult<Funko>();
    }
    [HttpPost]
    public ActionResult<Funko> Create([FromBody] Funko funko) {
        try {
            var nuevo =  repo.Add(funko);
            return CreatedAtAction(nameof(GetById), new { id = nuevo.Id }, funko);
        } catch (ArgumentException e) {
            return BadRequest(new { error = e.Message });
        }
    }

    [HttpPut("{id:int}")]
    public ActionResult<Funko> Update([FromBody] Funko funko, int id) {
        try {
            var act = repo.Update(id, funko);
            if (act is null) return NotFound();
            return Ok(act);
        }
        catch (ArgumentException e) {
            return BadRequest(new { error = e.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) {
        var del = repo.SoftDelete(id);
        if (!del) return NotFound();
        return NoContent();
    }
}
