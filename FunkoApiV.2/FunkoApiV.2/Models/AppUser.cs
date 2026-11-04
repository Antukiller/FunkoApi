using Microsoft.AspNetCore.Identity;

namespace FunkoApiV._2.Models;

public class AppUser : IdentityUser<long> {
    public string Nombre  { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public DateTime Createdt { get; set; } = DateTime.UtcNow;
}