using System.Security.Claims;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoApiV._2.Controller.Auth;
using FunkoApiV._2.Dto.Auth;
using FunkoApiV._2.Error;
using FunkoApiV._2.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Controllers;

[TestFixture]
[Category("Unit")]
public class AuthControllerTests {
    private Mock<IAuthService> _service = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void SetUp() {
        _service = new Mock<IAuthService>();
        _controller = new AuthController(_service.Object);
    }

    [Test]
    public async Task Login_ConCredencialesValidas_DevuelveOk() {
        var response = new AuthResponseDto("token", "user@example.com", "USER", DateTime.UtcNow);
        _service.Setup(x => x.Login(It.IsAny<LoginDto>()))
            .ReturnsAsync(Result.Success<AuthResponseDto, DomainError>(response));

        var result = await _controller.Login(new LoginDto("user@example.com", "Password1!"));

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Login_ConCredencialesInvalidas_DevuelveUnauthorized() {
        _service.Setup(x => x.Login(It.IsAny<LoginDto>()))
            .ReturnsAsync(Result.Failure<AuthResponseDto, DomainError>(new UnauthorizedError("invalid")));

        var result = await _controller.Login(new LoginDto("user@example.com", "bad"));

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public async Task Signup_Exitoso_DevuelveOk() {
        _service.Setup(x => x.Signup(It.IsAny<SignupDto>()))
            .ReturnsAsync(Result.Success<AuthResponseDto, DomainError>(
                new AuthResponseDto("token", "user@example.com", "USER", DateTime.UtcNow)));

        var result = await _controller.Signup(new SignupDto(
            "user@example.com", "Password1!", "User", "Name"));

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Signup_ConEmailExistente_DevuelveConflict() {
        _service.Setup(x => x.Signup(It.IsAny<SignupDto>()))
            .ReturnsAsync(Result.Failure<AuthResponseDto, DomainError>(new Conflict("exists")));

        var result = await _controller.Signup(new SignupDto(
            "user@example.com", "Password1!", "User", "Name"));

        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Test]
    public async Task GetMe_SinClaimDeUsuario_DevuelveUnauthorizedSinConsultarServicio() {
        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        var result = await _controller.GetMe();

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        _service.Verify(x => x.GetMe(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async Task GetMe_ConClaimNoNumerico_DevuelveUnauthorized() {
        _controller.ControllerContext = ControllerWithUserId("not-a-number");

        var result = await _controller.GetMe();

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        _service.Verify(x => x.GetMe(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async Task GetMe_ConUsuarioValido_DevuelveOk() {
        _controller.ControllerContext = ControllerWithUserId("42");
        _service.Setup(x => x.GetMe(42))
            .ReturnsAsync(Result.Success<UserDto, DomainError>(
                new UserDto(42, "user@example.com", "User", "Name", "USER")));

        var result = await _controller.GetMe();

        result.Result.Should().BeOfType<OkObjectResult>();
        _service.Verify(x => x.GetMe(42), Times.Once);
    }

    [Test]
    public async Task GetMe_UsuarioNoEncontrado_DevuelveNotFound() {
        _controller.ControllerContext = ControllerWithUserId("42");
        _service.Setup(x => x.GetMe(42))
            .ReturnsAsync(Result.Failure<UserDto, DomainError>(NotFound.FromId("42", "Usuario")));

        var result = await _controller.GetMe();

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    private static ControllerContext ControllerWithUserId(string userId) => new() {
        HttpContext = new DefaultHttpContext {
            User = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, userId)
            ], "test"))
        }
    };
}
