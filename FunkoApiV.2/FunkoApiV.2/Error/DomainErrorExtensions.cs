using Microsoft.AspNetCore.Mvc;
using FunkoApiV._2.Dto.Common;

namespace FunkoApiV._2.Error;

public static class DomainErrorExtensions {
    public static ActionResult<T> TopHttpResult<T>(this DomainError error) => error switch {
        NotFound => new NotFoundObjectResult(new ApiErrorResponse { Message = error.Message }),
        ValidationError => new BadRequestObjectResult(new ApiErrorResponse { Message = error.Message }),
        Conflict => new ConflictObjectResult(new ApiErrorResponse { Message = error.Message }),
        BusinessRuleError => new UnprocessableEntityObjectResult(new ApiErrorResponse { Message = error.Message }),
        UnauthorizedError => new UnauthorizedObjectResult(new ApiErrorResponse { Message = error.Message }),
        _ => new ObjectResult(new ApiErrorResponse { Message = "Error interno del servidor" }) { StatusCode = 500 }
    };
}
