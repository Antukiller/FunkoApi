using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoApiV._2.Controller;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Dto.Common;
using FunkoApiV._2.Error;
using FunkoApiV._2.Service;
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
            .ReturnsAsync(Result.Success<FunkoResponseDto, DomainError>(new FunkoResponseDto { Id = 1, Name = "Kirito" }));
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
}
