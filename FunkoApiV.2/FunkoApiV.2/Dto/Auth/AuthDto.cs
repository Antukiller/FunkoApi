namespace FunkoApiV._2.Dto.Auth;

public record LoginDto(string Email, string Password);
public record SignupDto(string Email, string Password, string Nombre, string Apellido);
public record AuthResponseDto(string Token, string Email, string Role, DateTime ExpiresAt);
public record UserDto(long id, string Email, string Nombre, string Apellidos, string Role);