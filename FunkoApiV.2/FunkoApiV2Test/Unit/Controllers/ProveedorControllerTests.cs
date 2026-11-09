using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoApiV._2.Controller;
using FunkoApiV._2.Dto.Proveedor;
using FunkoApiV._2.Error;
using FunkoApiV._2.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Controllers;

[TestFixture]
[Category("Unit")]
public class ProveedorControllerTests {
    private Mock<IProveedorService> _service = null!;
    private ProveedorController _controller = null!;
    private static ProveedorResponseDto Proveedor => new() {
        Id = "672f1a8b3c9d4e5f6a7b8c9d", Nombre = "Acme", Direccion = "Calle 1"
    };

    [SetUp]
    public void SetUp() {
        _service = new Mock<IProveedorService>();
        _controller = new ProveedorController(_service.Object);
    }

    [Test]
    public async Task GetAll_DevuelveListaOk() {
        _service.Setup(x => x.GetAll()).ReturnsAsync(new[] { Proveedor });

        (await _controller.GetAll()).Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetById_Existente_DevuelveOk() {
        _service.Setup(x => x.GetById(Proveedor.Id))
            .ReturnsAsync(Result.Success<ProveedorResponseDto, DomainError>(Proveedor));

        (await _controller.GetById(Proveedor.Id)).Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetById_Inexistente_DevuelveNotFound() {
        _service.Setup(x => x.GetById("missing"))
            .ReturnsAsync(Result.Failure<ProveedorResponseDto, DomainError>(
                NotFound.FromId("missing", "Proveedor")));

        (await _controller.GetById("missing")).Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task GetDeleted_DevuelveListaOk() {
        _service.Setup(x => x.GetDeleted()).ReturnsAsync(new[] { Proveedor });

        (await _controller.GetDeleted()).Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Create_Exitoso_DevuelveCreatedAtAction() {
        _service.Setup(x => x.Add(It.IsAny<ProveedorRequestDto>()))
            .ReturnsAsync(Result.Success<ProveedorResponseDto, DomainError>(Proveedor));

        (await _controller.Create(new ProveedorRequestDto())).Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Test]
    public async Task Create_ConErrorDeValidacion_DevuelveBadRequest() {
        _service.Setup(x => x.Add(It.IsAny<ProveedorRequestDto>()))
            .ReturnsAsync(Result.Failure<ProveedorResponseDto, DomainError>(new ValidationError("invalid")));

        (await _controller.Create(new ProveedorRequestDto())).Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Update_Exitoso_DevuelveOk() {
        _service.Setup(x => x.Update(Proveedor.Id, It.IsAny<ProveedorRequestDto>()))
            .ReturnsAsync(Result.Success<ProveedorResponseDto, DomainError>(Proveedor));

        (await _controller.Update(new ProveedorRequestDto(), Proveedor.Id)).Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Update_Inexistente_DevuelveNotFound() {
        _service.Setup(x => x.Update("missing", It.IsAny<ProveedorRequestDto>()))
            .ReturnsAsync(Result.Failure<ProveedorResponseDto, DomainError>(
                NotFound.FromId("missing", "Proveedor")));

        (await _controller.Update(new ProveedorRequestDto(), "missing")).Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task Delete_Exitoso_DevuelveNoContent() {
        _service.Setup(x => x.SoftDelete(Proveedor.Id))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        (await _controller.Delete(Proveedor.Id)).Result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task Delete_Inexistente_DevuelveNotFound() {
        _service.Setup(x => x.SoftDelete("missing"))
            .ReturnsAsync(Result.Failure<bool, DomainError>(NotFound.FromId("missing", "Proveedor")));

        (await _controller.Delete("missing")).Result.Should().BeOfType<NotFoundObjectResult>();
    }
}
