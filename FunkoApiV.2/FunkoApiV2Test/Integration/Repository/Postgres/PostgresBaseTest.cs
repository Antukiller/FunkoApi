using FunkoApiV._2.Entity;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FunkoApiV2Test.Repository.Postgres;

public abstract class PostgresBaseTest {
    private const string ProveedorId = "672f1a8b3c9d4e5f6a7b8c9d";

    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder("postgres:16-alpine").Build();

    protected AppDbContext Context { get; private set; } = null!;

    protected static string ProveedorSeedId => ProveedorId;

    [OneTimeSetUp]
    public async Task ClassSetUp() {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        Context = new AppDbContext(options);
    }

    [SetUp]
    public async Task SetUp() {
        Context.ChangeTracker.Clear();
        await Context.Database.ExecuteSqlRawAsync("DROP SCHEMA public CASCADE; CREATE SCHEMA public;");
        await Context.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task ClassTearDown() {
        await Context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
