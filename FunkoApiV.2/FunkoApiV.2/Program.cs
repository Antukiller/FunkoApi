using FluentValidation;
using FunkoApiMongo.Cache;
using FunkoApiMongo.Config;
using FunkoApiV._2.Config;
using FunkoApiV._2.Converters;
using FunkoApiV._2.Entity;
using FunkoApiV._2.Infrastructure;
using FunkoApiV._2.Infrastructure.Cassandra;
using FunkoApiV._2.Realtime;
using FunkoApiV._2.Seeders;
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
            .AddJsonOptions(options => {
                options.JsonSerializerOptions.Converters.Add(new ObjectIdJsonConverter());
            });
        
        builder.Services.AddIdentityConfiguration(builder.Configuration);
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services.AddOpenApi();
        builder.Services.AddRealtimeSignalR();
        builder.Services.AddCustomCors();

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

        // 8. OpenAPI siempre (ya no hay dos entornos)
        app.MapOpenApi();

        // 9. Esquema + seed de Postgres
        // 9. Esquema + seed de Postgres, Mongo e Identity
        using (var scope = app.Services.CreateScope()) {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dbMongo = scope.ServiceProvider.GetRequiredService<AppDbContextMongoDb>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var cassandraSession = scope.ServiceProvider.GetRequiredService<Cassandra.ISession>();
            var cassandraSettings = scope.ServiceProvider.GetRequiredService<CassandraSettings>();

            await cassandraSession.InitializeSchemaAsync(cassandraSettings.Keyspace);

            await db.Database.EnsureCreatedAsync();

            // Datos de negocio (categorías, proveedores, funkos)
            await DbSeeder.SeedAsync(db, dbMongo, logger);
            // Datos de negocio (categorías, proveedores, funkos)
            await DbSeeder.SeedAsync(db, dbMongo, logger);

            // Read model de Cassandra (se reconstruye desde Postgres + Mongo)
            await CassandraSeeder.SeedAsync(cassandraSession, db, dbMongo, logger);

            // Roles y usuarios de Identity (contraseña hasheada correctamente)
            await IdentitySeeder.SeedAsync(scope.ServiceProvider, logger);

            // Roles y usuarios de Identity (contraseña hasheada correctamente)
            await IdentitySeeder.SeedAsync(scope.ServiceProvider, logger);
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCustomCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<FunkosHub>("/hubs/funkos").RequireAuthorization();
        await app.RunAsync();
    }
}