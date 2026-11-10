using FunkoApiV._2.Entity;
using FunkoApiV._2.Models;
using FunkoApiV2Test.Integration;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Testcontainers.MongoDb;

namespace FunkoApiV2Test.Repository;

public abstract class ProveedorRepositoryBaseTest {
    protected static readonly ObjectId ProveedorId = ObjectId.Parse("672f1a8b3c9d4e5f6a7b8c9d");
    protected static readonly ObjectId ProveedorInexistenteId = ObjectId.Parse("672f1a8b3c9d4e5f6a7b8c9e");

    private MongoDbContainer? MongoDb;

    protected AppDbContextMongoDb DbContext { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task ClassSetUp() {
        await DockerTestPrerequisite.StartContainerOrIgnoreAsync(async () => {
            MongoDb = new MongoDbBuilder()
                .WithImage("mongo:7.0")
                .Build();
            await MongoDb.StartAsync();
        });

        var options = new DbContextOptionsBuilder<AppDbContextMongoDb>()
            .UseMongoDB(MongoDb!.GetConnectionString(), "test_database")
            .Options;

        DbContext = new AppDbContextMongoDb(options);
    }

    [OneTimeTearDown]
    public async Task ClassTearDown() {
        if (DbContext is not null)
            await DbContext.DisposeAsync();
        if (MongoDb is not null)
            await MongoDb.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp() {
        // Limpiar la colección
        var existentes = await DbContext.Proveedor.ToListAsync();
        DbContext.Proveedor.RemoveRange(existentes);
        await DbContext.SaveChangesAsync();

        // Sembrar el proveedor fijo que usan los tests
        DbContext.Proveedor.Add(new Proveedor {
            Id = ProveedorId,
            Nombre = "Proveedor",
            Direccion = "IES Luis Vives"
        });
        await DbContext.SaveChangesAsync();

        // El contexto es compartido por la fixture: no arrastres entidades trackeadas
        DbContext.ChangeTracker.Clear();
    }

    protected async Task<Proveedor> CreateTestProveedor(
        string nombre = "Test Proveedor",
        string direccion = "Calle 1") {
        var proveedor = new Proveedor {
            Nombre = nombre,
            Direccion = direccion,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        DbContext.Proveedor.Add(proveedor);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        return proveedor;
    }
}