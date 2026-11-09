using FluentAssertions;
using FunkoApiV._2.Error;
using FunkoApiV._2.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Storage;

[TestFixture]
[Category("Unit")]
public class FileSystemStorageServiceTests {
    private string _tempRoot = null!;
    private string _webRoot = null!;
    private FileSystemStorageService _storage = null!;

    [SetUp]
    public void SetUp() {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"FunkoStorageTests_{Guid.NewGuid():N}");
        _webRoot = Path.Combine(_tempRoot, "wwwroot");
        Directory.CreateDirectory(_webRoot);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["Storage:UploadPath"] = "uploads",
                ["Storage:MaxFileSize"] = "8",
                ["Storage:AllowedExtensions:0"] = ".png",
                ["Storage:AllowedContentTypes:0"] = "image/png"
            })
            .Build();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.WebRootPath).Returns(_webRoot);
        environment.SetupGet(x => x.ContentRootPath).Returns(_tempRoot);

        _storage = new FileSystemStorageService(
            configuration,
            NullLogger<FileSystemStorageService>.Instance,
            environment.Object);
    }

    [TearDown]
    public void TearDown() {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    [Test]
    public async Task SaveFileAsync_ArchivoValido_GuardaYDevuelveRutaPublica() {
        var result = await _storage.SaveFileAsync(CreateFile("funko.png"), "funkos");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("/uploads/funkos/");
        result.Value.Should().EndWith(".png");
        var savedFile = Path.Combine(_webRoot, result.Value.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        File.Exists(savedFile).Should().BeTrue();
        var savedBytes = await File.ReadAllBytesAsync(savedFile);
        savedBytes.Should().Equal(1, 2, 3);
    }

    [Test]
    public async Task SaveFileAsync_ArchivoVacio_DevuelveErrorDeNegocio() {
        var result = await _storage.SaveFileAsync(CreateFile("funko.png", []), "funkos");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<BusinessRuleError>();
        result.Error.Message.Should().Contain("vacío");
    }

    [Test]
    public async Task SaveFileAsync_ArchivoNulo_DevuelveErrorDeNegocio() {
        var result = await _storage.SaveFileAsync(null!, "funkos");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<BusinessRuleError>();
    }

    [Test]
    public async Task SaveFileAsync_ArchivoDemasiadoGrande_DevuelveErrorDeNegocio() {
        var result = await _storage.SaveFileAsync(CreateFile("funko.png", [1, 2, 3, 4, 5, 6, 7, 8, 9]), "funkos");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<BusinessRuleError>();
        result.Error.Message.Should().Contain("tamaño máximo");
    }

    [Test]
    public async Task SaveFileAsync_ExtensionNoPermitida_DevuelveErrorDeNegocio() {
        var result = await _storage.SaveFileAsync(CreateFile("funko.exe"), "funkos");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<BusinessRuleError>();
        result.Error.Message.Should().Contain("extensión");
    }

    [Test]
    public async Task SaveFileAsync_TipoDeContenidoNoPermitido_DevuelveErrorDeNegocio() {
        var result = await _storage.SaveFileAsync(CreateFile("funko.png", contentType: "text/plain"), "funkos");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<BusinessRuleError>();
        result.Error.Message.Should().Contain("contenido");
    }

    [Test]
    public async Task SaveFileAsync_CarpetaConTraversal_NoEscribeFueraDelAlmacenamiento() {
        var result = await _storage.SaveFileAsync(CreateFile("funko.png"), "../fuera");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InternalError>();
        Directory.Exists(Path.Combine(_tempRoot, "fuera")).Should().BeFalse();
    }

    [Test]
    public async Task DeleteFileAsync_ArchivoExistente_LoElimina() {
        var save = await _storage.SaveFileAsync(CreateFile("funko.png"), "funkos");
        save.IsSuccess.Should().BeTrue();

        var result = await _storage.DeleteFileAsync(save.Value);

        result.IsSuccess.Should().BeTrue();
        var savedFile = Path.Combine(_webRoot, save.Value.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        File.Exists(savedFile).Should().BeFalse();
    }

    [Test]
    public async Task DeleteFileAsync_RutaVacia_EsExitoso() {
        var result = await _storage.DeleteFileAsync(" ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Test]
    public async Task DeleteFileAsync_RutaFueraDelAlmacenamiento_DevuelveError() {
        var result = await _storage.DeleteFileAsync("../../fuera.png");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InternalError>();
    }

    private static FormFile CreateFile(
        string fileName,
        byte[]? content = null,
        string contentType = "image/png") {
        var bytes = content ?? [1, 2, 3];
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName) {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
