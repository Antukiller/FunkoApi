using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CSharpFunctionalExtensions;
using FunkoApiV._2.Config;
using FunkoApiV._2.Dto.Auth;
using FunkoApiV._2.Entity;
using FunkoApiV._2.Error;
using FunkoApiV._2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace FunkoApiV._2.Service;

public class AuthService(
    AppDbContext context, 
    UserManager<AppUser> userManager, 
    SignInManager<AppUser> signUser, 
    IOptions<JwtSettings> options) : IAuthService {
    
    public async Task<Result<AuthResponseDto, DomainError>> Login(LoginDto loginDto) {
        Log.Information("Email: {Email} logeandose", loginDto.Email);
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) {
            Log.Warning("Login fallido: Usuario con emais {Email} no encontrado", loginDto.Email);
            return Result.Failure<AuthResponseDto, DomainError>(new UnauthorizedError("Error de logging"));
        }
        var res = await signUser.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!res.Succeeded) {
            Log.Warning("Login fallido: Contraseña incorrecta");
            return Result.Failure<AuthResponseDto, DomainError>(new UnauthorizedError("Contraseña incorrecta"));
        }
        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? "USER";
        var token = GenerateJwtToken(user, role);

        Log.Information("Login exitoso para {Email} con rol {Role}", loginDto.Email, role);
        return Result.Success<AuthResponseDto, DomainError>(token);
    }

    public async Task<Result<AuthResponseDto, DomainError>> Signup(SignupDto signupDto) {
        Log.Information("Intento de registro para {Email}", signupDto.Email);

        var existingUser = await userManager.FindByEmailAsync(signupDto.Email);
        if (existingUser is not null) {
            Log.Warning("Registro fallido: {Email} ya existe", signupDto.Email);
            return Result.Failure<AuthResponseDto, DomainError>(
                new Conflict($"El usuario con email {signupDto.Email} ya existe"));
        }
        var user = new AppUser {
            UserName = signupDto.Email,
            Email = signupDto.Email,
            Nombre = signupDto.Nombre,
            Apellido = signupDto.Apellido,
            Createdt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, signupDto.Password);
        if (!result.Succeeded) {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            Log.Warning("Registro fallido para {Email}: {Errors}", signupDto.Email, errors);
            return Result.Failure<AuthResponseDto, DomainError>(
                new BusinessRuleError($"Error al crear el usuario: {errors}"));
        }

        await userManager.AddToRoleAsync(user, "USER");

        var role = "USER";
        var token = GenerateJwtToken(user, role);

        Log.Information("Registro exitoso para {Email} con rol {Role}", signupDto.Email, role);
        return Result.Success<AuthResponseDto, DomainError>(token);
    }

    public async Task<Result<UserDto, DomainError>> GetMe(long userId) {
        Log.Debug("Consultando usuario {UserId}", userId);

        var user = await context.Users.FindAsync(userId);
        if (user is null) {
            return Result.Failure<UserDto, DomainError>(
                NotFound.FromId(userId.ToString(), "Usuario"));
        }
        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? "USER";
        return Result.Success<UserDto, DomainError>(
            new UserDto(user.Id, user.Email ?? "", user.Nombre, user.Apellido, role));
    }
    
    private AuthResponseDto GenerateJwtToken(AppUser user, string role) {
        var settings = options.Value;
        var expires = DateTime.UtcNow.AddMinutes(settings.ExpirationMin);

        var claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Name, $"{user.Nombre} {user.Apellido}".Trim())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponseDto(tokenString, user.Email ?? "", role, expires);
    }
}