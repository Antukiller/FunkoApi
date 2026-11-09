using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoApiV._2.Controller;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;
using FunkoApiV._2.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Controllers;

[TestFixture]
[Category("Unit")]
public class FunkoControllerTests {
    private Mock<IFunkoService> _service = null!;
    private FunkoController _controller = null!;

    [SetUp]
    public void SetUp() {
        _service = new Mock<IFunkoService>();
        _controller = new FunkoController(_service.Object);
    }

    [Test]
    public async Task GetById_CuandoExiste_Devuelve200() {
        _service.Setup(s => s.GetById(1))
            .ReturnsAsync(Result.Success<FunkoResponseDto, DomainError>(
                new FunkoResponseDto(1, "Kirito", 0, 0, null, "", "", "", default, null)));
        var r = await _controller.GetById(1);
        r.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetById_CuandoNoExiste_Devuelve404() {
        _service.Setup(s => s.GetById(99))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, DomainError>(NotFound.FromId("99", "Funko")));
        var r = await _controller.GetById(99);
        r.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task Delete_CuandoExiste_Devuelve204() {
        _service.Setup(s => s.Delete(1))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));
        var r = await _controller.Delete(1);
        r.Result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task GetAll_DevuelveOkConPagina() {
        _service.Setup(s => s.GetAll(It.IsAny<FunkoFilterRequest>()))
            .ReturnsAsync(new PagedResult<FunkoResponseDto> { Items = [], TotalCount = 0, Page = 0, PageSize = 10 });
        var r = await _controller.GetAll(new FunkoFilterRequest());
        r.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Create_Exitoso_DevuelveCreatedAtAction() {
        _service.Setup(s => s.Add(It.IsAny<FunkoCreateRequest>()))
            .ReturnsAsync(Result.Success<FunkoResponseDto, DomainError>(
                new FunkoResponseDto(1, "", 0, 0, null, "", "", "", default, null)));

        (await _controller.Create(new FunkoCreateRequest())).Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Test]
    public async Task Create_ConErrorDeValidacion_DevuelveBadRequest() {
        _service.Setup(s => s.Add(It.IsAny<FunkoCreateRequest>()))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, DomainError>(new ValidationError("invalid")));

        (await _controller.Create(new FunkoCreateRequest())).Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Update_Exitoso_DevuelveOk() {
        _service.Setup(s => s.Update(1, It.IsAny<FunkoUpdateRequest>()))
            .ReturnsAsync(Result.Success<FunkoResponseDto, DomainError>(
                new FunkoResponseDto(1, "", 0, 0, null, "", "", "", default, null)));

        (await _controller.Update(1, new FunkoUpdateRequest())).Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Update_Inexistente_DevuelveNotFound() {
        _service.Setup(s => s.Update(99, It.IsAny<FunkoUpdateRequest>()))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, DomainError>(NotFound.FromId("99", "Funko")));

        (await _controller.Update(99, new FunkoUpdateRequest())).Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task Delete_Inexistente_DevuelveNotFound() {
        _service.Setup(s => s.Delete(99))
            .ReturnsAsync(Result.Failure<bool, DomainError>(NotFound.FromId("99", "Funko")));

        (await _controller.Delete(99)).Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task UploadImagen_Exitoso_DevuelveRuta() {
        _service.Setup(s => s.UploadImagen(It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Success<string, DomainError>("/uploads/funkos/a.png"));

        (await _controller.UploadImagen(new Mock<IFormFile>().Object)).Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task UploadImagen_Invalido_DevuelveUnprocessableEntity() {
        _service.Setup(s => s.UploadImagen(It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Failure<string, DomainError>(new BusinessRuleError("invalid")));

        (await _controller.UploadImagen(new Mock<IFormFile>().Object)).Result.Should().BeOfType<UnprocessableEntityObjectResult>();
    }

    [Test]
    public async Task UpdateImagen_Exitoso_DevuelveOk() {
        _service.Setup(s => s.UpdateImagen(1, It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Success<FunkoResponseDto, DomainError>(
                new FunkoResponseDto(1, "", 0, 0, null, "", "", "", default, null)));

        (await _controller.UpdateImagen(1, new Mock<IFormFile>().Object)).Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task UpdateImagen_Inexistente_DevuelveNotFound() {
        _service.Setup(s => s.UpdateImagen(99, It.IsAny<IFormFile>()))
            .ReturnsAsync(Result.Failure<FunkoResponseDto, DomainError>(NotFound.FromId("99", "Funko")));

        (await _controller.UpdateImagen(99, new Mock<IFormFile>().Object)).Result.Should().BeOfType<NotFoundObjectResult>();
    }
}
