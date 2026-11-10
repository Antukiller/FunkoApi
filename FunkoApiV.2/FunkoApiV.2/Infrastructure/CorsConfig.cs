namespace FunkoApiV._2.Infrastructure;

public static class CorsConfig {
    public const string PolicyName = "FunkoCorsPolicy";
    public static IServiceCollection AddCustomCors(this IServiceCollection services) {
        services.AddCors(options => {
            options.AddPolicy(PolicyName, policy => {
                policy.WithOrigins(
                        "http://localhost:5500",
                        "http://127.0.0.1:5500",
                        "http://localhost:63342",
                        "http://localhost:5197",
                        "http://127.0.0.1:5197"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        return services;
    }
    public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app) {
        return app.UseCors(PolicyName);
    }
}