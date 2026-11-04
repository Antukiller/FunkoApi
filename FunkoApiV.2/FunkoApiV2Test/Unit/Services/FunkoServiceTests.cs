using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Error;
using FunkoApiV._2.Models;
using FunkoApiV._2.Repository;
using FunkoApiV._2.Service;
using FunkoApiV._2.Storage;
using FunkoApiV._2.Validator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MongoDB.Bson;

namespace FunkoApiV2Test.Unit.Services;

[TestFixture]
[Category("Unit")]
public class FunkoServiceTests {
    private const string ProveedorId = "672f1a8b3c9d4e5f6a7b8c9d";

    private Mock<IFunkoRepository> _repo = null!;
    private Mock<IProveedorRepository> _repoProveedor = null!;
    private Mock<ICategoriaRepository> _repoCategoria = null!;
    private Mock<IStorageService> _storage = null!;
    private FunkoService _service = null!;

    private static Proveedor ProveedorSeed => new() {
        Id = ObjectId.Parse(ProveedorId), Nombre = "Proveedor Test", CreatedAt = DateTime.UtcNow, IsDeleted = false
    };
    private static Categoria CategoriaSeed => new() {
        Id = "Anime000001", Nombre = "Anime", CreatedAt = DateTime.UtcNow, IsDeleted = false
    };

    [SetUp]
    public void SetUp() {
        _repo = new Mock<IFunkoRepository>();
        _repoProveedor = new Mock<IProveedorRepository>();
        _repoCategoria = new Mock<ICategoriaRepository>();
        _storage = new Mock<IStorageService>();

        _repoCategoria.Setup(x => x.GetById("Anime000001"))
            .ReturnsAsync(Result.Success<Categoria, DomainError>(CategoriaSeed));
        _repoCategoria.Setup(x => x.GetById(It.Is<string>(s => s != "Anime000001")))
            .ReturnsAsync(Result.Failure<Categoria, DomainError>(NotFound.FromId("x", "Categoria")));
        _repoProveedor.Setup(x => x.GetById(ProveedorId))
            .ReturnsAsync(Result.Success<Proveedor, DomainError>(ProveedorSeed));
        _repoProveedor.Setup(x => x.GetById(It.Is<string>(s => s != ProveedorId)))
            .ReturnsAsync(Result.Failure<Proveedor, DomainError>(NotFound.FromId("x", "Proveedor")));

        _service = new FunkoService(
            _repo.Object,
            _repoProveedor.Object,
            _repoCategoria.Object,
            NullLogger<FunkoService>.Instance,
            new FunkoCreateValidator(),
            new FunkoUpdateValidator(),
            _storage.Object);
    }

    [Test]
    public async Task Add_ConImagenQueNoEsRutaRelativa_DevuelveValidationError() {
        var request = CreateRequest() with { Imagen = "https://ejemplo.com/funko.png" };
        var resultado = await _service.Add(request);
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<ValidationError>();
        _repo.Verify(x => x.CreateAsync(It.IsAny<Funko>()), Times.Never);
    }

    [Test]
    public async Task Add_ConProveedorQueNoExiste_DevuelveConflict() {
        var request = CreateRequest() with { ProveedorId = "000000000000000000000000" };
        var resultado = await _service.Add(request);
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<Conflict>();
        _repo.Verify(x => x.CreateAsync(It.IsAny<Funko>()), Times.Never);
    }

    [Test]
    public async Task Add_ConCategoriaQueNoExiste_DevuelveConflict() {
        var request = CreateRequest() with { CategoriaId = "NoExiste001" };
        var resultado = await _service.Add(request);
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<Conflict>();
        _repo.Verify(x => x.CreateAsync(It.IsAny<Funko>()), Times.Never);
    }

    [Test]
    public async Task Add_ConDatosValidos_DevuelveDto() {
        var creado = new Funko {
            Id = 1, Name = "Kirito", Precio = 15.99m, Stock = 10,
            Imagen = "/uploads/funkos/a.png", CategoriaId = "Anime000001",
            ProveedorId = ProveedorId,
            Categoria = new Categoria { Id = "Anime000001", Nombre = "Anime" },
            CreatedAt = DateTime.UtcNow
        };
        _repo.Setup(x => x.CreateAsync(It.IsAny<Funko>()))
            .ReturnsAsync(Result.Success<Funko, DomainError>(creado));

        var resultado = await _service.Add(CreateRequest());

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Id.Should().Be(1);
        resultado.Value.Categoria.Should().Be("Anime");
    }

    [Test]
    public async Task GetById_Inexistente_DevuelveNotFound() {
        _repo.Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync(Result.Failure<Funko, DomainError>(NotFound.FromId("99", "Funko")));
        var resultado = await _service.GetById(99);
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<NotFound>();
    }

    [Test]
    public async Task Update_ConFunkoEliminado_DevuelveConflict() {
        var eliminado = new Funko {
            Id = 1, Name = "Kirito", Precio = 15.99m, Stock = 10,
            CategoriaId = "Anime000001", ProveedorId = ProveedorId, IsDeleted = true
        };
        _repo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Funko, DomainError>(eliminado));

        var resultado = await _service.Update(1, new FunkoUpdateRequest {
            Name = "Kirito", Precio = 15.99m, Stock = 10,
            CategoriaId = "Anime000001", ProveedorId = ProveedorId
        });

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<Conflict>();
    }

    [Test]
    public async Task UpdateImagen_GuardaLaNuevaYBorraLaAnterior() {
        var anterior = new Funko {
            Id = 1, CategoriaId = "Anime000001", ProveedorId = ProveedorId,
            Imagen = "/uploads/funkos/vieja.png"
        };
        var actualizado = anterior with { Imagen = "/uploads/funkos/nueva.png" };

        _repo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Funko, DomainError>(anterior));
        _storage.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "funkos"))
            .ReturnsAsync(Result.Success<string, DomainError>("/uploads/funkos/nueva.png"));
        _repo.Setup(x => x.SetImagenAsync(1, "/uploads/funkos/nueva.png"))
            .ReturnsAsync(Result.Success<Funko, DomainError>(actualizado));

        var resultado = await _service.UpdateImagen(1, new Mock<IFormFile>().Object);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Imagen.Should().Be("/uploads/funkos/nueva.png");
        _storage.Verify(x => x.DeleteFileAsync("/uploads/funkos/vieja.png"), Times.Once);
    }

    [Test]
    public async Task UpdateImagen_SiSetImagenFalla_LimpiaLaImagenNueva() {
        var anterior = new Funko { Id = 1, CategoriaId = "Anime000001", ProveedorId = ProveedorId, Imagen = "/uploads/funkos/vieja.png" };
        _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Result.Success<Funko, DomainError>(anterior));
        _storage.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "funkos"))
            .ReturnsAsync(Result.Success<string, DomainError>("/uploads/funkos/nueva.png"));
        _repo.Setup(x => x.SetImagenAsync(1, "/uploads/funkos/nueva.png"))
            .ReturnsAsync(Result.Failure<Funko, DomainError>(NotFound.FromId("1", "Funko")));

        var resultado = await _service.UpdateImagen(1, new Mock<IFormFile>().Object);

        resultado.IsFailure.Should().BeTrue();
        _storage.Verify(x => x.DeleteFileAsync("/uploads/funkos/nueva.png"), Times.Once);
    }

    [Test]
    public async Task UploadImagen_CuandoElFicheroNoEsValido_DevuelveBusinessRuleError() {
        _storage.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "funkos"))
            .ReturnsAsync(Result.Failure<string, DomainError>(StorageError.ExtensionNoPermitida()));
        var resultado = await _service.UploadImagen(new Mock<IFormFile>().Object);
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<BusinessRuleError>();
    }

    private static FunkoCreateRequest CreateRequest() => new() {
        Name = "Kirito", Precio = 15.99m, Stock = 10,
        CategoriaId = "Anime000001", ProveedorId = ProveedorId,
        Imagen = "/uploads/funkos/a.png"
    };
}
