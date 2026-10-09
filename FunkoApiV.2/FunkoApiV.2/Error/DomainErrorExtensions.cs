using Microsoft.AspNetCore.Mvc;

namespace FunkoApiV._2.Error;

public static class DomainErrorExtensions {
    public static ActionResult<T> TopHttpResult<T>(this DomainError error) => error switch {
        NotFound => new NotFoundObjectResult(new { message = error }),
        ValidationError ve => new BadRequestObjectResult(new { message = ve.Message, errors = ve.Errors }),
        _ => new ObjectResult(new { message = "Error interno del servidor" }) { StatusCode = 500 }
    };
}