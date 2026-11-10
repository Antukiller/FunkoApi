using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoApiMongo.Cache;
using FunkoApiV._2.Dto.Proveedor;
using FunkoApiV._2.Error;
using FunkoApiV._2.Service;
using FunkoApiV._2.Service.Proveedor;
using Moq;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Services;

[TestFixture]
[Category("Unit")]
public class CachedProveedorServiceTests {
    private Mock<IProveedorService> _inner = null!;
    private Mock<ICacheService> _cache = null!;
    private CachedProveedorService _sut = null!;
    private static ProveedorResponseDto Dto => new() { Id = "provider-1", Nombre = "Acme", Direccion = "Calle 1" };

    [SetUp]
    public void SetUp() {
        _inner = new Mock<IProveedorService>();
        _cache = new Mock<ICacheService>();
        _sut = new CachedProveedorService(_inner.Object, _cache.Object);
    }

    [Test]
    public async Task GetAll_CacheVacio_DevuelveListaVacia() {
        _cache.Setup(x => x.GetOrSetAsync<IReadOnlyList<ProveedorResponseDto>>(
                "proveedor:all", It.IsAny<Func<Task<IReadOnlyList<ProveedorResponseDto>?>>>(),
                default, null))
            .ReturnsAsync((IReadOnlyList<ProveedorResponseDto>?)null);

        (await _sut.GetAll()).Should().BeEmpty();
        _cache.Verify(x => x.GetOrSetAsync<IReadOnlyList<ProveedorResponseDto>>(
            "proveedor:all", It.IsAny<Func<Task<IReadOnlyList<ProveedorResponseDto>?>>>(), default, null), Times.Once);
    }

    [Test]
    public async Task GetDeleted_CacheVacio_DevuelveListaVacia() {
        _cache.Setup(x => x.GetOrSetAsync<IReadOnlyCollection<ProveedorResponseDto>>(
                "proveedor:deleted", It.IsAny<Func<Task<IReadOnlyCollection<ProveedorResponseDto>?>>>(),
                default, null))
            .ReturnsAsync((IReadOnlyCollection<ProveedorResponseDto>?)null);

        (await _sut.GetDeleted()).Should().BeEmpty();
    }

    [Test]
    public async Task GetById_CacheHit_NoLlamaAlServicioInterno() {
        _cache.Setup(x => x.GetAsync<ProveedorResponseDto>("proveedor:provider-1", default)).ReturnsAsync(Dto);

        var result = await _sut.GetById("provider-1");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Dto);
        _inner.Verify(x => x.GetById(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetById_CacheMiss_GuardaResultadoExitoso() {
        _cache.Setup(x => x.GetAsync<ProveedorResponseDto>("proveedor:provider-1", default))
            .ReturnsAsync((ProveedorResponseDto?)null);
        _inner.Setup(x => x.GetById("provider-1"))
            .ReturnsAsync(Result.Success<ProveedorResponseDto, DomainError>(Dto));

        (await _sut.GetById("provider-1")).IsSuccess.Should().BeTrue();
        _cache.Verify(x => x.SetAsync("proveedor:provider-1", Dto, null, default), Times.Once);
    }

    [Test]
    public async Task GetById_ErrorInterno_NoGuardaEnCache() {
        _cache.Setup(x => x.GetAsync<ProveedorResponseDto>("missing", default))
            .ReturnsAsync((ProveedorResponseDto?)null);
        _inner.Setup(x => x.GetById("missing"))
            .ReturnsAsync(Result.Failure<ProveedorResponseDto, DomainError>(NotFound.FromId("missing")));

        (await _sut.GetById("missing")).IsFailure.Should().BeTrue();
        _cache.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<ProveedorResponseDto>(), null, default), Times.Never);
    }

    [Test]
    public async Task Add_Exitoso_InvalidaListaCompleta() {
        var request = new ProveedorRequestDto { Nombre = "Acme", Direccion = "Calle 1" };
        _inner.Setup(x => x.Add(request))
            .ReturnsAsync(Result.Success<ProveedorResponseDto, DomainError>(Dto));

        (await _sut.Add(request)).IsSuccess.Should().BeTrue();
        _cache.Verify(x => x.RemoveAsync("proveedor:all", default), Times.Once);
    }

    [Test]
    public async Task Add_Falla_NoInvalidaCache() {
        var request = new ProveedorRequestDto();
        _inner.Setup(x => x.Add(request))
            .ReturnsAsync(Result.Failure<ProveedorResponseDto, DomainError>(new Conflict("error")));

        (await _sut.Add(request)).IsFailure.Should().BeTrue();
        _cache.Verify(x => x.RemoveAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Test]
    public async Task Update_Exitoso_InvalidaProveedorYLista() {
        var request = new ProveedorRequestDto { Nombre = "Acme", Direccion = "Calle 1" };
        _inner.Setup(x => x.Update("provider-1", request))
            .ReturnsAsync(Result.Success<ProveedorResponseDto, DomainError>(Dto));

        (await _sut.Update("provider-1", request)).IsSuccess.Should().BeTrue();
        _cache.Verify(x => x.RemoveAsync("proveedor:provider-1", default), Times.Once);
        _cache.Verify(x => x.RemoveAsync("proveedor:all", default), Times.Once);
    }

    [Test]
    public async Task Update_Falla_NoInvalidaCache() {
        _inner.Setup(x => x.Update("missing", It.IsAny<ProveedorRequestDto>()))
            .ReturnsAsync(Result.Failure<ProveedorResponseDto, DomainError>(NotFound.FromId("missing")));

        (await _sut.Update("missing", new ProveedorRequestDto())).IsFailure.Should().BeTrue();
        _cache.Verify(x => x.RemoveAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Test]
    public async Task SoftDelete_Exitoso_InvalidaProveedorAmbasListas() {
        _inner.Setup(x => x.SoftDelete("provider-1")).ReturnsAsync(Result.Success<bool, DomainError>(true));

        (await _sut.SoftDelete("provider-1")).IsSuccess.Should().BeTrue();
        _cache.Verify(x => x.RemoveAsync("proveedor:provider-1", default), Times.Once);
        _cache.Verify(x => x.RemoveAsync("proveedor:all", default), Times.Once);
        _cache.Verify(x => x.RemoveAsync("proveedor:deleted", default), Times.Once);
    }

    [Test]
    public async Task SoftDelete_Falla_NoInvalidaCache() {
        _inner.Setup(x => x.SoftDelete("missing"))
            .ReturnsAsync(Result.Failure<bool, DomainError>(NotFound.FromId("missing")));

        (await _sut.SoftDelete("missing")).IsFailure.Should().BeTrue();
        _cache.Verify(x => x.RemoveAsync(It.IsAny<string>(), default), Times.Never);
    }
}
