using FluentValidation;
using FunkoApiV._2.Dto.Common;
using FunkoApiMongo.Cache;
using FunkoApiMongo.Config;
using FunkoApiV._2.Config;
using FunkoApiV._2.Converters;
using FunkoApiV._2.Entity;
using FunkoApiV._2.Infrastructure;
using FunkoApiV._2.Infrastructure.Cassandra;
using FunkoApiV._2.Realtime;
using FunkoApiV._2.Seeders;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using Serilog;

namespace FunkoApiV._2;

public class Program {

    public static async Task Main(string[] args) {

        var builder = WebApplication.CreateBuilder(args);

        // 0. Inicializar la configuración (¡antes de usarla!)
        FunkoConfig.Init(builder.Configuration);

        // 1. Serilog
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        // 3. Controladores + OpenAPI
        builder.Services.AddControllers()
            .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new ObjectIdJsonConverter()); });
        builder.Services.Configure<ApiBehaviorOptions>(options => {
            options.InvalidModelStateResponseFactory = context => {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? "El valor proporcionado no es válido."
                                : error.ErrorMessage)
                            .ToArray());
                return new BadRequestObjectResult(new ApiErrorResponse {
                    Message = "La solicitud contiene errores de validación.",
                    Errors = errors
                });
            };
        });

        builder.Services.AddIdentityConfiguration(builder.Configuration);
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        builder.Services.AddGraphQLConfig(builder.Environment);

        builder.Services.AddOpenApi();
        builder.Services.AddRealtimeSignalR();
        builder.Services.AddCustomCors(builder.Configuration);
        builder.Services.AddSwaggerGen(options => {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo {
                Title = "Funko API",
                Version = "v1",
                Description = "API REST para autenticación, Funkos, categorías, proveedores y consultas de Cassandra."
            });
            var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
            options.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile));
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme {
                Name = "Authorization",
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.ParameterLocation.Header,
                Description = "Introduce el token JWT sin el prefijo Bearer."
            });
            options.OperationFilter<AuthorizeOperationFilter>();
            options.OperationFilter<ApiResponsesOperationFilter>();
        });
        
        var permitLimit = Math.Max(1, builder.Configuration.GetValue("RateLimiting:PermitLimit", 120));
        var windowSeconds = Math.Max(1, builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60));
        builder.Services.AddRateLimiter(options => {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            options.OnRejected = async (context, cancellationToken) => {
                context.HttpContext.Response.Headers["Retry-After"] = windowSeconds.ToString();
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { message = "Se ha superado el límite de peticiones. Inténtalo más tarde." },
                    cancellationToken);
            };
        });

        // 4. EF Core: Postgres (Funko/Categoria) + Mongo (Proveedor)
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(FunkoConfig.PostgresConnection));

        builder.Services.AddDbContext<AppDbContextMongoDb>(options =>
            options.UseMongoDB(FunkoConfig.MongoConnection, FunkoConfig.MongoDatabase));

        builder.Services.AddCassandra(builder.Configuration);


        // 5. Caché (Redis, sin bifurcar por entorno)
        builder.Services.Configure<CacheConfig>(
            builder.Configuration.GetSection(CacheConfig.CacheName));

        builder.Services.AddStackExchangeRedisCache(options =>
            options.Configuration = FunkoConfig.RedisConnection);

        builder.Services.AddSingleton<ICacheService, RedisService>();

        // 6. Scrutor (repositorios, servicios y decoradores)
        builder.Services.AddDependencies();

        var app = builder.Build();

        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Funko API v1"));
        }

        // 9. Inicializar esquemas y, solo en desarrollo, cargar datos de ejemplo.
        using (var scope = app.Services.CreateScope()) {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dbMongo = scope.ServiceProvider.GetRequiredService<AppDbContextMongoDb>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var cassandraSession = scope.ServiceProvider.GetRequiredService<Cassandra.ISession>();
            var cassandraSettings = scope.ServiceProvider.GetRequiredService<CassandraSettings>();

            await cassandraSession.InitializeSchemaAsync(cassandraSettings.Keyspace);

            await db.Database.EnsureCreatedAsync();
            await PostgresSchemaInitializer.EnsureSchemaAsync(db);

            // Roles y usuarios de Identity (esto SIEMPRE, para poder loguearte en prod)
            await IdentitySeeder.SeedAsync(scope.ServiceProvider, logger);

            // Datos de prueba (funkos, categorías, proveedores): SOLO EN DESARROLLO
            if (app.Environment.IsDevelopment()) {
                await DbSeeder.SeedAsync(db, dbMongo, logger);
                await CassandraSeeder.SeedAsync(cassandraSession, db, dbMongo, logger);
            }
        }

            if (!app.Environment.IsDevelopment()) {
                app.Use(async (context, next) => {
                    context.Response.OnStarting(() => {
                        context.Response.Headers["Content-Security-Policy"] =
                            "default-src 'self'; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
                        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                        context.Response.Headers["X-Frame-Options"] = "DENY";
                        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                        return Task.CompletedTask;
                    });
                    await next();
                });
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCustomCors();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWebSockets();
            app.MapControllers();
            app.MapHub<FunkosHub>("/hubs/funkos").RequireAuthorization();
            app.MapGraphQLEndpoints();

            await app.RunAsync();
    }
}
