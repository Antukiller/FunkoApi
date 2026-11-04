using System.Text;
using FunkoApiV._2.Config;
using FunkoApiV._2.Entity;
using FunkoApiV._2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace FunkoApiV._2.Infrastructure;

public static class IdentityConfig
{
    /// <summary>
    /// Registra Identity, JWT Bearer y políticas de autorización.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la app.</param>
    /// <returns>IServiceCollection para encadenar.</returns>
    public static IServiceCollection AddIdentityConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Log.Information("Configurando Identity y JWT...");

        // ── JWT Settings ─────────────────────────────────────────────────────
        var jwtSection = configuration.GetSection(JwtSettings.Section);
        services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();

        if (string.IsNullOrWhiteSpace(jwtSettings.Key))
            throw new InvalidOperationException("JWT Key no configurada en appsettings.json");

        // ── Identity ─────────────────────────────────────────────────────────
        // AddIdentityCore: Identity sin UI (no registra la cookie Identity.Application),
        // el mecanismo oficial para APIs.
        services.AddIdentityCore<AppUser>(options =>
        {
            // Password
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;

            // Lockout
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;

            // User
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole<long>>()
        .AddSignInManager()
        .AddEntityFrameworkStores<AppDbContext>()  
        .AddDefaultTokenProviders();

        // ── JWT Authentication ───────────────────────────────────────────────
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // ── Authorization Policies ───────────────────────────────────────────
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdmin", policy =>
                policy.RequireRole("ADMIN").Build())
            .AddPolicy("RequireUser", policy =>
                policy.RequireRole("USER", "ADMIN").Build());

        Log.Information("Identity y JWT configurados correctamente.");
        return services;
    }
}