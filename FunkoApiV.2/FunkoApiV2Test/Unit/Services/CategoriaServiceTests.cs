using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Error;
using FunkoApiV._2.Models;
using FunkoApiV._2.Repository;
using FunkoApiV._2.Service;
using FunkoApiV._2.Validator;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FunkoApiV2Test.Unit.Services;

[TestFixture]
public class CategoriaServiceTests {
    private Mock<ICategoriaRepository> _repo = null!;
    private Mock<IFunkoRepository> _funkoRepo = null!;
    private CategoriaService _service = null!;

    [SetUp]
    public void SetUp() {
        _repo = new Mock<ICategoriaRepository>();
        _funkoRepo = new Mock<IFunkoRepository>();
        _service = new CategoriaService(
            _repo.Object,
            _funkoRepo.Object,
            NullLogger<CategoriaService>.Instance,
            new CategoriaValidator());
    }
    [Test]
    public async Task Add_ConNombreVacio_DevuelveValidationError() {
        var resultado = await _service.Add(new CategoriaRequestDto { Nombre = "" });

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<ValidationError>();
        _repo.Verify(x => x.CreateAsync(It.IsAny<Categoria>()), Times.Never);
    }

    [Test]
    public async Task Add_ConDatosValidos_DevuelveDto() {
        var creada = new Categoria {
            Id = "Anime000001",
            Nombre = "Anime",
            Descripcion = "Anime y manga",
            CreatedAt = DateTime.UtcNow
        };
        _repo.Setup(x => x.CreateAsync(It.IsAny<Categoria>()))
            .ReturnsAsync(Result.Success<Categoria, DomainError>(creada));

        var resultado = await _service.Add(new CategoriaRequestDto { Nombre = "Anime", Descripcion = "Anime y manga" });

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Id.Should().Be("Anime000001");
        resultado.Value.Nombre.Should().Be("Anime");
    }

    [Test]
    public async Task GetAll_DevuelvePaginaConTotal() {
        var categorias = new List<Categoria> {
            new() { Id = "Anime000001", Nombre = "Anime", CreatedAt = DateTime.UtcNow }
        };
        _repo.Setup(x => x.GetPagedAsync(It.IsAny<CategoriaFilterRequest>()))
            .ReturnsAsync((categorias, 1));

        var resultado = await _service.GetAll(new CategoriaFilterRequest { Page = 0, Size = 10 });

        resultado.TotalCount.Should().Be(1);
        resultado.Items.Should().HaveCount(1);
        resultado.TotalPages.Should().Be(1);
        resultado.HasNextPage.Should().BeFalse();
    }
}
