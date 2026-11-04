namespace FunkoApiV._2.Config;

public class JwtSettings {
    public const string Section = "Jwt";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMin { get; set; }
}