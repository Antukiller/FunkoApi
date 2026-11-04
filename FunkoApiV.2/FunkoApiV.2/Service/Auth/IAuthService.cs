using CSharpFunctionalExtensions;
using FunkoApiV._2.Dto.Auth; 
using FunkoApiV._2.Error;
using Microsoft.AspNetCore.Identity;

namespace FunkoApiV._2.Service; // Ajusta según tu namespace

/// <summary>
/// Define las operaciones del servicio de autenticación y gestión de usuarios.
/// </summary>
public interface IAuthService {
    /// <summary>
    /// Autentica un usuario con credenciales locales (email/usuario y contraseña).
    /// </summary>
    /// <param name="loginDto">Datos de inicio de sesión (credenciales del usuario).</param>
    /// <returns>
    /// Un <see cref="Result{AuthResponseDto, DomainError}"/> que contiene el DTO de respuesta de autenticación
    /// con el token y datos del usuario, o un <see cref="DomainError"/> si las credenciales son inválidas.
    /// </returns>
    Task<Result<AuthResponseDto, DomainError>> Login(LoginDto loginDto);
    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="signupDto">Datos de registro del nuevo usuario.</param>
    /// <returns>
    /// Un <see cref="Result{AuthResponseDto, DomainError}"/> con el DTO de respuesta de autenticación
    /// tras el registro, o un <see cref="DomainError"/> si el email/usuario ya existe o los datos son inválidos.
    /// </returns>
    Task<Result<AuthResponseDto, DomainError>> Signup(SignupDto signupDto);

    /// <summary>
    /// Obtiene los datos del usuario autenticado a partir de su identificador.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>
    /// Un <see cref="Result{UserDto, DomainError}"/> con los datos del usuario, o un <see cref="DomainError"/>
    /// si el usuario no existe.
    /// </returns>
    Task<Result<UserDto, DomainError>> GetMe(long userId);
}