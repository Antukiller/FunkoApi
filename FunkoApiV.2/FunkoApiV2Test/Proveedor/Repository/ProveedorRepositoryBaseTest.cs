using FunkoApiV._2.Entity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Testcontainers.MongoDb;

namespace FunkoApiV2Test.Proveedor.Repository;

public abstract class ProveedorRepositoryBaseTest {
    protected static readonly ObjectId ProveedorId = ObjectId.Parse("672f1a8b3c9d4e5f6a7b8c9d");
    protected static readonly ObjectId ProveedorInexistenteId = ObjectId.Parse("672f1a8b3c9d4e5f6a7b8c9e");

    private readonly MongoDbContainer MongoDb = new MongoDbBuilder()
        .WithImage("mongo:7.0")
        .Build();

    protected AppDbContextMongoDb DbContext { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task ClassSetUp() {
        await MongoDb.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContextMongoDb>()
            .UseMongoDB(MongoDb.GetConnectionString(), "test_database")
            .Options;

        DbContext = new AppDbContextMongoDb(options);
    }

    [OneTimeTearDown]
    public async Task ClassTearDown() {
        await DbContext.DisposeAsync();
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