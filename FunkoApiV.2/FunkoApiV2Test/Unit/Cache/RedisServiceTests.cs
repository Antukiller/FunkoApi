using FluentAssertions;
using FunkoApiMongo.Cache;
using FunkoApiMongo.Config;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Cache;

[TestFixture]
[Category("Unit")]
public class RedisServiceTests {
    private InMemoryDistributedCache _cache = null!;
    private RedisService _sut = null!;

    [SetUp]
    public void SetUp() {
        _cache = new InMemoryDistributedCache();
        _sut = new RedisService(_cache, Options.Create(new CacheConfig {
            ExpiryMinutes = 20,
            SlidingExpirationMinutes = 5
        }));
    }

    [Test]
    public async Task GetAsync_ClaveAusente_DevuelveDefault() {
        (await _sut.GetAsync<string>("missing")).Should().BeNull();
    }

    [Test]
    public async Task GetAsync_ClaveExistente_DeserializaValor() {
        await _cache.SetStringAsync("item", """{"Name":"Kirito"}""");

        (await _sut.GetAsync<Item>("item")).Should().Be(new Item("Kirito"));
    }

    [Test]
    public async Task SetAsync_GuardaValorSerializado() {
        await _sut.SetAsync("item", new Item("Kirito"));

        (await _cache.GetStringAsync("item")).Should().Be("""{"Name":"Kirito"}""");
    }

    [Test]
    public async Task RemoveAsync_EliminaValor() {
        await _cache.SetStringAsync("item", "\"value\"");

        await _sut.RemoveAsync("item");

        (await _cache.GetStringAsync("item")).Should().BeNull();
    }

    [Test]
    public async Task ExistsAsync_IndicaPresenciaDeUnValor() {
        await _cache.SetStringAsync("item", "\"value\"");

        (await _sut.ExistsAsync("item")).Should().BeTrue();
        (await _sut.ExistsAsync("missing")).Should().BeFalse();
    }

    [Test]
    public async Task GetOrSetAsync_CacheHit_NoEjecutaFactory() {
        await _cache.SetStringAsync("item", "\"cached\"");
        var factoryCalls = 0;

        var value = await _sut.GetOrSetAsync("item", () => {
            factoryCalls++;
            return Task.FromResult<string?>("new");
        });

        value.Should().Be("cached");
        factoryCalls.Should().Be(0);
    }

    [Test]
    public async Task GetOrSetAsync_CacheMiss_GuardaResultadoFactory() {
        var value = await _sut.GetOrSetAsync("item", () => Task.FromResult<string?>("created"));

        value.Should().Be("created");
        (await _sut.GetAsync<string>("item")).Should().Be("created");
    }

    [Test]
    public async Task GetOrSetAsync_FactoryDevuelveNull_NoGuardaEntrada() {
        var value = await _sut.GetOrSetAsync<string>("item", () => Task.FromResult<string?>(null));

        value.Should().BeNull();
        (await _cache.GetStringAsync("item")).Should().BeNull();
    }

    [Test]
    public async Task GetOrSetAsync_FactoryFalla_LiberaElBloqueoParaLaSiguienteLlamada() {
        var calls = 0;
        var failed = () => _sut.GetOrSetAsync<string>("item", () => {
            calls++;
            throw new InvalidOperationException("factory failed");
        });

        await failed.Should().ThrowAsync<InvalidOperationException>();
        (await _sut.GetOrSetAsync("item", () => {
            calls++;
            return Task.FromResult<string?>("recovered");
        })).Should().Be("recovered");
        calls.Should().Be(2);
    }

    public sealed record Item(string Name);

    private sealed class InMemoryDistributedCache : IDistributedCache {
        private readonly Dictionary<string, byte[]> _entries = new(StringComparer.Ordinal);

        public byte[]? Get(string key) => _entries.GetValueOrDefault(key);

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
            Task.FromResult(Get(key));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
            _entries[key] = value;

        public Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken token = default) {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key) { }

        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

        public void Remove(string key) => _entries.Remove(key);

        public Task RemoveAsync(string key, CancellationToken token = default) {
            Remove(key);
            return Task.CompletedTask;
        }
    }
}
