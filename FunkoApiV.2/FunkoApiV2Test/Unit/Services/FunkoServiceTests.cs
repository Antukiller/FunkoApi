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

namespace FunkoApiV2Test.Unit.Services;

[TestFixture]
public class FunkoServiceTests {
    private const string ProveedorId = "672f1a8b3c9d4e5f6a7b8c9d";

    private Mock<IFunkoRepository> _repo = null!;
    private Mock<IFileStorageService> _storage = null!;
    private FunkoService _service = null!;
    private Mock<IProveedorRepository> _repoProveedor;
    private Mock<ICategoriaRepository> _repoCategoria;
    
    [SetUp]
    public void SetUp() {
        _repo = new Mock<IFunkoRepository>();
        _storage = new Mock<IFileStorageService>();
        _repoProveedor = new Mock<IProveedorRepository>();
        _repoCategoria = new Mock<ICategoriaRepository>();
    
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
    public async Task Add_ConProveedorQueNoEsObjectId_DevuelveValidationError() {
        var request = CreateRequest() with { ProveedorId = "123" };

        var resultado = await _service.Add(request);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<ValidationError>();
    }

    [Test]
    public async Task Add_ConDatosValidos_DevuelveDto() {
        var creado = new Funko {
            Id = 1,
            Name = "Kirito",
            Precio = 15.99m,
            Stock = 10,
            Imagen = "/uploads/funkos/a.png",
            CategoriaId = "Anime000001",
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
    public async Task UpdateImagen_GuardaLaNuevaYBorraLaAnterior() {
        var anterior = new Funko {
            Id = 1,
            CategoriaId = "Anime000001",
            ProveedorId = ProveedorId,
            Imagen = "/uploads/funkos/vieja.png"
        };
        var actualizado = anterior with { Imagen = "/uploads/funkos/nueva.png" };

        _repo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Funko, DomainError>(anterior));
        _storage.Setup(x => x.SaveAsync(It.IsAny<IFormFile>(), "funkos"))
            .ReturnsAsync("/uploads/funkos/nueva.png");
        _repo.Setup(x => x.SetImagenAsync(1, "/uploads/funkos/nueva.png"))
            .ReturnsAsync(Result.Success<Funko, DomainError>(actualizado));

        var resultado = await _service.UpdateImagen(1, new Mock<IFormFile>().Object);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Imagen.Should().Be("/uploads/funkos/nueva.png");
        _storage.Verify(x => x.Delete("/uploads/funkos/vieja.png"), Times.Once);
    }

    [Test]
    public async Task UploadImagen_CuandoElFicheroNoEsValido_DevuelveValidationError() {
        _storage.Setup(x => x.SaveAsync(It.IsAny<IFormFile>(), "funkos"))
            .ThrowsAsync(new ArgumentException("Formato no permitido"));

        var resultado = await _service.UploadImagen(new Mock<IFormFile>().Object);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<ValidationError>();
    }

    private static FunkoCreateRequest CreateRequest() => new() {
        Name = "Kirito",
        Precio = 15.99m,
        Stock = 10,
        CategoriaId = "Anime000001",
        ProveedorId = ProveedorId,
        Imagen = "/uploads/funkos/a.png"
    };
}
