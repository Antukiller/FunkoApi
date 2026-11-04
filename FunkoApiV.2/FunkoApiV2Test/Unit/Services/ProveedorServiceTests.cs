using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Proveedor;
using FunkoApiV._2.Error;
using FunkoApiV._2.Models;
using FunkoApiV._2.Repository;
using FunkoApiV._2.Service;
using FunkoApiV._2.Validator.Proveedor;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MongoDB.Bson;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Services;

[TestFixture]
[Category("Unit")]
public class ProveedorServiceTests {
    private Mock<IProveedorRepository> _repo = null!;
    private Mock<IFunkoRepository> _funkoRepo = null!;
    private ProveedorService _service = null!;

    private static Proveedor ProveedorSeed => new() {
        Id = ObjectId.GenerateNewId(), Nombre = "Acme", Direccion = "Calle 1",
        CreatedAt = DateTime.UtcNow, IsDeleted = false
    };

    [SetUp]
    public void SetUp() {
        _repo = new Mock<IProveedorRepository>();
        _funkoRepo = new Mock<IFunkoRepository>();
        _service = new ProveedorService(_repo.Object, _funkoRepo.Object,
            NullLogger<ProveedorService>.Instance, new ProveedorValidator());
    }

    [Test]
    public async Task GetById_ConIdExistente_DevuelveDto() {
        var p = ProveedorSeed;
        _repo.Setup(r => r.GetById(p.Id.ToString()))
            .ReturnsAsync(Result.Success<Proveedor, DomainError>(p));
        var r = await _service.GetById(p.Id.ToString());
        r.IsSuccess.Should().BeTrue();
        r.Value.Nombre.Should().Be("Acme");
    }

    [Test]
    public async Task GetById_ConIdInexistente_DevuelveNotFound() {
        _repo.Setup(r => r.GetById("000000000000000000000000"))
            .ReturnsAsync(Result.Failure<Proveedor, DomainError>(NotFound.FromId("000000000000000000000000", "Proveedor")));
        var r = await _service.GetById("000000000000000000000000");
        r.IsFailure.Should().BeTrue();
        r.Error.Should().BeOfType<NotFound>();
    }

    [Test]
    public async Task Add_ConDatosValidos_DevuelveDto() {
        var p = ProveedorSeed;
        _repo.Setup(r => r.Add(It.IsAny<Proveedor>()))
            .ReturnsAsync(Result.Success<Proveedor, DomainError>(p));
        var r = await _service.Add(new ProveedorRequestDto { Nombre = "Acme", Direccion = "Calle 1" });
        r.IsSuccess.Should().BeTrue();
        r.Value.Nombre.Should().Be("Acme");
    }

    [Test]
    public async Task Add_ConNombreVacio_DevuelveValidationError() {
        var r = await _service.Add(new ProveedorRequestDto { Nombre = "", Direccion = "Calle 1" });
        r.IsFailure.Should().BeTrue();
        r.Error.Should().BeOfType<ValidationError>();
        _repo.Verify(r => r.Add(It.IsAny<Proveedor>()), Times.Never);
    }

    [Test]
    public async Task SoftDelete_ConFunkosAsociados_DevuelveConflict() {
        var p = ProveedorSeed;
        _funkoRepo.Setup(f => f.GetPagedAsync(It.IsAny<FunkoFilterRequest>()))
            .ReturnsAsync((new List<Funko> { new() { Id = 1 } }, 1));
        var r = await _service.SoftDelete(p.Id.ToString());
        r.IsFailure.Should().BeTrue();
        r.Error.Should().BeOfType<Conflict>();
        _repo.Verify(r => r.SoftDelete(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SoftDelete_SinFunkos_DevuelveTrue() {
        var p = ProveedorSeed;
        _funkoRepo.Setup(f => f.GetPagedAsync(It.IsAny<FunkoFilterRequest>()))
            .ReturnsAsync((new List<Funko>(), 0));
        _repo.Setup(r => r.SoftDelete(p.Id.ToString()))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));
        var r = await _service.SoftDelete(p.Id.ToString());
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().BeTrue();
    }
}
