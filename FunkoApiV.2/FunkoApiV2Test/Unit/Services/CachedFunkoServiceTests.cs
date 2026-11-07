using CSharpFunctionalExtensions;
using FluentAssertions;
using FunkoApiMongo.Cache;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Error;
using FunkoApiV._2.Service;
using Moq;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Services;

[TestFixture]
[Category("Unit")]
public class CachedFunkoServiceTests {
    private Mock<IFunkoService> _inner = null!;
    private Mock<ICacheService> _cache = null!;
    private CachedFunkoService _sut = null!;

    [SetUp]
    public void SetUp() {
        _inner = new Mock<IFunkoService>();
        _cache = new Mock<ICacheService>();
        _sut = new CachedFunkoService(_inner.Object, _cache.Object);
    }

    [Test]
    public async Task GetById_CuandoHayCache_DevuelveCacheYNoLlamaAlInner() {
        var cached = new FunkoResponseDto(1, "Kirito", 0, 0, null, "", "", "", default, null);
        _cache.Setup(c => c.GetAsync<FunkoResponseDto>("funko:1")).ReturnsAsync(cached);
        var r = await _sut.GetById(1);
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(cached);
        _inner.Verify(s => s.GetById(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetById_CuandoNoHayCache_LlamaAlInnerYGuardaEnCache() {
        var dto = new FunkoResponseDto(1, "Kirito", 0, 0, null, "", "", "", default, null);
        _cache.Setup(c => c.GetAsync<FunkoResponseDto>("funko:1")).ReturnsAsync((FunkoResponseDto?)null);
        _inner.Setup(s => s.GetById(1)).ReturnsAsync(Result.Success<FunkoResponseDto, DomainError>(dto));
        var r = await _sut.GetById(1);
        r.IsSuccess.Should().BeTrue();
        _cache.Verify(c => c.SetAsync("funko:1", dto, null, default), Times.Once);
    }

    [Test]
    public async Task Update_CuandoEsExitoso_InvalidaLaCache() {
        _inner.Setup(s => s.Update(1, It.IsAny<FunkoUpdateRequest>()))
            .ReturnsAsync(Result.Success<FunkoResponseDto, DomainError>(
                new FunkoResponseDto(1, "", 0, 0, null, "", "", "", default, null)));
        await _sut.Update(1, new FunkoUpdateRequest());
        _cache.Verify(c => c.RemoveAsync("funko:1", default), Times.Once);
    }

    [Test]
    public async Task Delete_CuandoEsExitoso_InvalidaLaCache() {
        _inner.Setup(s => s.Delete(1)).ReturnsAsync(Result.Success<bool, DomainError>(true));
        await _sut.Delete(1);
        _cache.Verify(c => c.RemoveAsync("funko:1", default), Times.Once);
    }
}
