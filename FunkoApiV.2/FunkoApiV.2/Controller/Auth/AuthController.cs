using System.Security.Claims;
using FunkoApiV._2.Dto.Auth;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;      
using FunkoApiV._2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FunkoApiV._2.Controller.Auth;

/// <summary>
/// Controller de autenticación: login, signup y perfil.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase {
    /// <summary>
    /// Autentica al usuario y devuelve un JWT para las siguientes operaciones protegidas.
    /// </summary>
    /// <remarks>Envía email y contraseña en JSON. Las credenciales incorrectas devuelven 401.</remarks>
    /// <response code="200">Autenticación correcta; devuelve token, email, rol y caducidad.</response>
    /// <response code="400">JSON inválido o campos requeridos ausentes.</response>
    /// <response code="401">Email o contraseña incorrectos.</response>
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto) {
        var result = await authService.Login(dto);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<AuthResponseDto>();
    }

    /// <summary>
    /// Registra un usuario con rol USER e inicia sesión devolviendo un JWT.
    /// </summary>
    /// <remarks>La contraseña debe tener al menos 8 caracteres e incluir mayúscula, minúscula, número y símbolo.</remarks>
    /// <response code="200">Usuario creado; devuelve token, email, rol y caducidad.</response>
    /// <response code="400">JSON inválido o campos requeridos ausentes.</response>
    /// <response code="409">Ya existe una cuenta con ese email.</response>
    /// <response code="422">La contraseña no cumple las reglas de Identity u otro requisito de negocio.</response>
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponseDto>> Signup([FromBody] SignupDto dto) {
        var result = await authService.Signup(dto);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<AuthResponseDto>();
    }

    /// <summary>
    /// Devuelve el perfil asociado al token JWT actual.
    /// </summary>
    /// <response code="200">Perfil y rol del usuario.</response>
    /// <response code="401">Token ausente, inválido o sin identificador de usuario.</response>
    /// <response code="404">El usuario del token ya no existe.</response>
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe() {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!long.TryParse(userIdClaim, out long userId)) {
            return Unauthorized(new ApiErrorResponse {
                Message = "Token inválido o usuario no identificado"
            });
        }

        var result = await authService.GetMe(userId);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<UserDto>();
    }
}