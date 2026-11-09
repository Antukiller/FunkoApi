# 📄 doc/04 — Controladores REST

---

## 1. Estructura del controller

```csharp
[ApiController]
[Route("api/funkos")]
[Produces("application/json")]
public class FunkoController(
    IFunkoService service,
    ILogger<FunkoController> logger
) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult<FunkoResponseDto>> GetById(int id)
    {
        logger.LogInformation("Obteniendo funko con ID: {Id}", id);
        var resultado = await service.GetById(id);
        return resultado.Match(
            onSuccess: funko => funko,
            onFailure: error => error.TopHttpResult<FunkoResponseDto>()
        );
    }
}
```

---

## 2. Reglas de los controladores

| Regla | Detalle |
|-------|---------|
| Retorno tipado | `Task<ActionResult<TDto>>`, nunca `IActionResult` sin tipar |
| Éxito | `Match(onSuccess, onFailure)` con `TopHttpResult<T>()` centralizado |
| Created | `CreatedAtAction(nameof(GetById), new { id = ... }, dto)` |
| Borrado | `NoContent()` en éxito |
| Documentación | `[ProducesResponseType]` por cada código de estado |
| Autorización | `[Authorize(Policy = "RequireAdminRole")]` / `[AllowAnonymous]` explícita |
| Un controller por agregado | `FunkoController` → `IFunkoService`, nunca mezcla servicios |

---

## 3. Traducción de errores a HTTP

Extensión centralizada en `Error/DomainErrorExtensions.cs`:

```csharp
public static ActionResult<T> TopHttpResult<T>(this DomainError error) => error switch
{
    NotFound => new NotFoundObjectResult(new { message = error.Message }),
    ValidationError => new BadRequestObjectResult(new { message = error.Message }),
    Conflict => new ConflictObjectResult(new { message = error.Message }),
    BusinessRuleError => new UnprocessableEntityObjectResult(new { message = error.Message }),
    UnauthorizedError => new UnauthorizedObjectResult(new { message = error.Message }),
    _ => new ObjectResult(new { message = "Error interno del servidor" }) { StatusCode = 500 }
};
```

**Códigos HTTP:**
| Error de dominio | HTTP | Cuándo |
|------------------|------|--------|
| `NotFoundError` | 404 | Recurso inexistente |
| `ValidationError` | 400 | Fallo de validación |
| `ConflictError` | 409 | Duplicado / conflicto de estado |
| `BusinessRuleError` | 422 | Regla de negocio incumplida |
| `UnauthorizedError` | 401 | Sin autenticar |
| `InternalError` | 500 | Fallo del servidor |

---

## 4. Endpoints por entidad

### Funko (`/api/funkos`)
- `GET` — Listar paginado con filtros (público)
- `GET {id}` — Obtener por ID (Admin)
- `POST` — Crear (Admin)
- `PUT {id}` — Actualizar (Admin)
- `DELETE {id}` — Borrado lógico (Admin)
- `POST /imagen` — Subir imagen
- `PATCH {id}/imagen` — Actualizar imagen

### Categoría (`/api/categorias`)
- `GET` — Listar paginado (público)
- `GET {id}` — Obtener por ID (público)
- `POST` — Crear (Admin)
- `PUT {id}` — Actualizar (Admin)
- `DELETE {id}` — Borrado lógico con validación de funkos asociados (Admin)

### Proveedor (`/api/Proveedor`) — MongoDB
- `GET` — Listar (público)
- `GET {id}` — Obtener por ID (público)
- `POST` — Crear (Admin)
- `PUT {id}` — Actualizar (Admin)
- `DELETE {id}` — Eliminar (Admin)