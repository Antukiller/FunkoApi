using FluentAssertions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Error;
using FunkoApiV._2.Models;
using FunkoApiV._2.Repository;

namespace FunkoApiV2Test.Repository.Postgres;

[TestFixture]
public class FunkoRepositoryTests : PostgresBaseTest {
    private FunkoRepository _repository = null!;

    [SetUp]
    public async Task SetUpRepository() {
        _repository = new FunkoRepository(Context);
        var fecha = DateTime.UtcNow;
        Context.Funkos.AddRange(
            NuevoFunko("Anime000001") with { Name = "Kirito", Precio = 15.99m, Stock = 10, CreatedAt = fecha },
            NuevoFunko("Anime000001") with { Name = "Hinata Shoyo", Precio = 14.50m, Stock = 8, CreatedAt = fecha },
            NuevoFunko("Videojuego1") with { Name = "Master Chief", Precio = 19.99m, Stock = 5, CreatedAt = fecha },
            NuevoFunko("Videojuego1") with { Name = "Geralt de Rivia", Precio = 17.50m, Stock = 12, CreatedAt = fecha },
            NuevoFunko("Peliculas01") with { Name = "Darth Vader", Precio = 21.00m, Stock = 3, CreatedAt = fecha });
        await Context.SaveChangesAsync();
    }

    [Test]
    public async Task GetPagedAsync_devuelve_los_funkos_sembrados_paginados() {
        var (items, total) = await _repository.GetPagedAsync(new FunkoFilterRequest { Page = 0, Size = 2 });

        total.Should().Be(5);
        items.Should().HaveCount(2);
    }

    [Test]
    public async Task GetPagedAsync_filtra_por_categoria() {
        var (items, total) = await _repository.GetPagedAsync(
            new FunkoFilterRequest { CategoriaId = "Anime000001" });

        total.Should().Be(2);
        items.Should().OnlyContain(f => f.CategoriaId == "Anime000001");
    }

    [Test]
    public async Task GetPagedAsync_filtra_por_precio_y_ordenacion() {
        var (items, total) = await _repository.GetPagedAsync(new FunkoFilterRequest {
            PrecioMax = 18m,
            SortBy = "precio",
            Direction = "desc"
        });

        total.Should().Be(3);
        items.Select(f => f.Precio).Should().BeInDescendingOrder();
        items.Should().OnlyContain(f => f.Precio <= 18m);
    }

    [Test]
    public async Task CreateAsync_devuelve_el_funko_con_la_categoria_cargada() {
        var resultado = await _repository.CreateAsync(NuevoFunko("Anime000001"));

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Id.Should().BePositive();
        resultado.Value.Categoria.Nombre.Should().Be("Anime");
        resultado.Value.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Test]
    public async Task DeleteAsync_lo_marca_como_borrado() {
        var resultado = await _repository.DeleteAsync(1);

        resultado.IsSuccess.Should().BeTrue();
        (await _repository.GetByIdAsync(1)).IsFailure.Should().BeTrue();
        (await _repository.GetPagedAsync(new FunkoFilterRequest())).Total.Should().Be(4);
    }

    [Test]
    public async Task SetImagenAsync_actualiza_la_ruta_relativa() {
        var resultado = await _repository.SetImagenAsync(1, "/uploads/funkos/nueva.png");

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Imagen.Should().Be("/uploads/funkos/nueva.png");
    }
    

    private static Funko NuevoFunko(string categoriaId) => new() {
        Name = "Nuevo",
        Precio = 10m,
        Stock = 2,
        Imagen = "/uploads/funkos/nuevo.png",
        CategoriaId = categoriaId,
        ProveedorId = ProveedorSeedId
    };
}
