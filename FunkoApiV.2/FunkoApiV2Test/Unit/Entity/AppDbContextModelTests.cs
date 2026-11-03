using FluentAssertions;
using FunkoApiV._2.Entity;
using FunkoApiV._2.Models;
using Microsoft.EntityFrameworkCore;

namespace FunkoApiV2Test.Unit.Entity;

[TestFixture]
public class AppDbContextModelTests {
    private AppDbContext _context = null!;

    [SetUp]
    public void SetUp() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=funko_test;Username=test;Password=test")
            .Options;
        _context = new AppDbContext(options);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public void El_modelo_se_construye_y_solo_mapea_las_entidades_SQL() {
        var entidades = _context.Model.GetEntityTypes().Select(e => e.ClrType).ToList();

        entidades.Should().Contain([typeof(Funko), typeof(Categoria)]);
        // Proveedor vive en Mongo: no debe existir como entidad en PostgreSQL.
        entidades.Should().NotContain(typeof(Proveedor));
    }

    [Test]
    public void El_id_del_proveedor_se_guarda_como_texto_de_24_caracteres() {
        var propiedad = _context.Model.FindEntityType(typeof(Funko))!
            .FindProperty(nameof(Funko.ProveedorId))!;

        propiedad.ClrType.Should().Be<string>();
        propiedad.GetMaxLength().Should().Be(24);
    }
}
