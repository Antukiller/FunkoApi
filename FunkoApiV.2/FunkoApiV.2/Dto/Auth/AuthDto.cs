using System.ComponentModel.DataAnnotations;

namespace FunkoApiV._2.Dto.Auth;

/// <summary>Credenciales para iniciar sesión.</summary>
/// <param name="Email">Dirección de correo registrada.</param>
/// <param name="Password">Contraseña de la cuenta.</param>
public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password);

/// <summary>Datos necesarios para registrar una cuenta de usuario.</summary>
/// <param name="Email">Dirección de correo válida y única.</param>
/// <param name="Password">Mínimo 8 caracteres con mayúscula, minúscula, número y símbolo.</param>
/// <param name="Nombre">Nombre del usuario.</param>
/// <param name="Apellido">Apellido del usuario.</param>
public record SignupDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8), RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$")] string Password,
    [Required, MinLength(1)] string Nombre,
    [Required, MinLength(1)] string Apellido);

/// <summary>Token JWT emitido tras iniciar sesión o registrarse.</summary>
/// <param name="Token">Token de acceso Bearer.</param>
/// <param name="Email">Correo del usuario.</param>
/// <param name="Role">Rol asignado al usuario.</param>
/// <param name="ExpiresAt">Fecha y hora UTC de expiración.</param>
public record AuthResponseDto(string Token, string Email, string Role, DateTime ExpiresAt);

/// <summary>Perfil público del usuario autenticado.</summary>
public record UserDto(long id, string Email, string Nombre, string Apellidos, string Role);