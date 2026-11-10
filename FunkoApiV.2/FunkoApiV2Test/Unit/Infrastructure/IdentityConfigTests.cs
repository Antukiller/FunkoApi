using FluentAssertions;
using FunkoApiV._2.Config;
using FunkoApiV._2.Entity;
using FunkoApiV._2.Infrastructure;
using FunkoApiV._2.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Infrastructure;

[TestFixture]
[Category("Unit")]
public class IdentityConfigTests {
    private const string Key = "identity-config-test-key-that-is-at-least-32-bytes";

    [Test]
    public void AddIdentityConfiguration_SinClaveJwt_LanzaErrorExplicito() {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        var action = () => services.AddIdentityConfiguration(configuration);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT Key no configurada en appsettings.json");
    }

    [Test]
    public async Task AddIdentityConfiguration_ClaveJwtValida_RegistraIdentityYPoliticas() {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=:memory:"));
        var configuration = Configuration(new Dictionary<string, string?> {
            ["Jwt:Key"] = Key,
            ["Jwt:Issuer"] = "tests",
            ["Jwt:Audience"] = "tests",
            ["Jwt:ExpirationMin"] = "30"
        });

        services.AddIdentityConfiguration(configuration);
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<UserManager<AppUser>>().Should().NotBeNull();
        provider.GetRequiredService<SignInManager<AppUser>>().Should().NotBeNull();
        var identity = provider.GetRequiredService<IOptions<IdentityOptions>>().Value;
        identity.Password.RequiredLength.Should().Be(8);
        identity.Password.RequireDigit.Should().BeTrue();
        identity.Password.RequireUppercase.Should().BeTrue();
        identity.Password.RequireLowercase.Should().BeTrue();
        identity.Password.RequireNonAlphanumeric.Should().BeTrue();
        identity.Lockout.MaxFailedAccessAttempts.Should().Be(5);
        identity.User.RequireUniqueEmail.Should().BeTrue();

        var authentication = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);
        authentication.TokenValidationParameters.ValidateIssuer.Should().BeTrue();
        authentication.TokenValidationParameters.ValidateAudience.Should().BeTrue();
        authentication.TokenValidationParameters.ValidateLifetime.Should().BeTrue();
        authentication.TokenValidationParameters.ValidateIssuerSigningKey.Should().BeTrue();
        authentication.TokenValidationParameters.ValidIssuer.Should().Be("tests");
        authentication.TokenValidationParameters.ValidAudience.Should().Be("tests");

        var policies = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        var adminRoles = (await policies.GetPolicyAsync("RequireAdmin"))!.Requirements
            .OfType<RolesAuthorizationRequirement>().Single().AllowedRoles;
        var userRoles = (await policies.GetPolicyAsync("RequireUser"))!.Requirements
            .OfType<RolesAuthorizationRequirement>().Single().AllowedRoles;
        adminRoles.Should().Contain("ADMIN");
        userRoles.Should().Contain(["USER", "ADMIN"]);
    }

    private static IConfiguration Configuration(IDictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
