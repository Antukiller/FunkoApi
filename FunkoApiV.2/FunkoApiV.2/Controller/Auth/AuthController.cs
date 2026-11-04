using System.Security.Claims;
using FunkoApiV._2.Dto.Auth;
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
    /// Inicia sesión con email y contraseña.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto) {
        var result = await authService.Login(dto);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<AuthResponseDto>();
    }

    /// <summary>
    /// Registra un nuevo usuario.
    /// </summary>
    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponseDto>> Signup([FromBody] SignupDto dto) {
        var result = await authService.Signup(dto);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<AuthResponseDto>();
    }

    /// <summary>
    /// Obtiene la información del usuario autenticado.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe() {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!long.TryParse(userIdClaim, out long userId)) {
            return Unauthorized(new { message = "Token inválido o usuario no identificado" });
        }

        var result = await authService.GetMe(userId);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.TopHttpResult<UserDto>();
    }
}