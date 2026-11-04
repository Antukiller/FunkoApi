using CSharpFunctionalExtensions;
using FunkoApiV._2.Error;
using FunkoApiV._2.Infrastructure.Interfaces;

namespace FunkoApiV._2.Storage;

public class FileSystemStorageService : IStorageService, ITransientService
{
    private readonly string _rootPath;
    private readonly string _uploadPath;
    private readonly long _maxFileSize;
    private readonly HashSet<string> _allowedExtensions;
    private readonly HashSet<string> _allowedContentTypes;
    private readonly ILogger<FileSystemStorageService> _logger;

    public FileSystemStorageService(IConfiguration configuration, ILogger<FileSystemStorageService> logger, IWebHostEnvironment env)
    {
        _logger = logger;

        var configuredUploadPath = (configuration["Storage:UploadPath"] ?? "uploads")
            .Replace('\\', '/')
            .Trim('/');
        _uploadPath = configuredUploadPath.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase)
            ? configuredUploadPath["wwwroot/".Length..]
            : configuredUploadPath;
        _maxFileSize = configuration.GetValue<long>("Storage:MaxFileSize", 5 * 1024 * 1024);
        _allowedExtensions = (configuration.GetSection("Storage:AllowedExtensions").Get<string[]>()
            ?? [".jpg", ".jpeg", ".png", ".gif"])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _allowedContentTypes = (configuration.GetSection("Storage:AllowedContentTypes").Get<string[]>()
            ?? ["image/jpeg", "image/png", "image/gif"])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var webRoot = env.WebRootPath ?? System.IO.Path.Combine(env.ContentRootPath, "wwwroot");
        _rootPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(webRoot, _uploadPath));

        if (!Directory.Exists(_rootPath))
            Directory.CreateDirectory(_rootPath);

        _logger.LogInformation("Storage service inicializado en: {Path}", _rootPath);
    }

    private DomainError? ValidateFile(IFormFile file) {
        if (file is null or { Length: 0 })
            return StorageError.ArchivoVacio();
        if (file.Length > _maxFileSize)
            return StorageError.ArchivoMuyGrande();
        if (string.IsNullOrWhiteSpace(file.FileName))
            return StorageError.NombreArchivoInvalido();

        var extension = System.IO.Path.GetExtension(file.FileName);
        if (!_allowedExtensions.Contains(extension))
            return StorageError.ExtensionNoPermitida();
        if (string.IsNullOrWhiteSpace(file.ContentType) ||
            !_allowedContentTypes.Contains(file.ContentType))
            return StorageError.TipoContenidoNoPermitido();

        return null;
    }

    public async Task<Result<string, DomainError>> SaveFileAsync(IFormFile file, string folder) {
        var validationError = ValidateFile(file);
        if (validationError is not null)
            return Result.Failure<string, DomainError>(validationError);

        string? filePath = null;
        try
        {
            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            var filename = $"{Guid.NewGuid():N}{extension}";
            var relativePath = GetRelativePath(filename, folder);
            filePath = GetFullPath(relativePath);
            var folderPath = System.IO.Path.GetDirectoryName(filePath)!;
            Directory.CreateDirectory(folderPath);

            await using var stream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);
            await file.CopyToAsync(stream);

            _logger.LogInformation("Archivo guardado: {Path}", relativePath);
            return Result.Success<string, DomainError>(relativePath);
        }
        catch (Exception ex)
        {
            if (filePath is not null && File.Exists(filePath)) {
                try {
                    File.Delete(filePath);
                }
                catch (Exception cleanupException) {
                    _logger.LogError(cleanupException, "No se pudo limpiar el archivo incompleto {Path}", filePath);
                }
            }
            _logger.LogError(ex, "Error guardando archivo");
            return Result.Failure<string, DomainError>(StorageError.ErrorGuardando());
        }
    }

    public Task<Result<bool, DomainError>> DeleteFileAsync(string relativePath) {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.FromResult(Result.Success<bool, DomainError>(true));

        try
        {
            var fullPath = GetFullPath(relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Archivo eliminado: {Filename}", relativePath);
            }
            return Task.FromResult(Result.Success<bool, DomainError>(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando archivo {Filename}", relativePath);
            return Task.FromResult(Result.Failure<bool, DomainError>(StorageError.ErrorEliminando()));
        }
    }

    private string GetFullPath(string filename) {
        var relativePath = filename.Replace('\\', '/').TrimStart('/');
        var uploadPrefix = $"{_uploadPath.TrimEnd('/')}/";
        if (relativePath.StartsWith(uploadPrefix, StringComparison.OrdinalIgnoreCase))
            relativePath = relativePath[uploadPrefix.Length..];

        var fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(
            _rootPath,
            relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar)));
        var rootPrefix = _rootPath.TrimEnd(
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("La ruta debe estar dentro del almacenamiento configurado.", nameof(filename));

        return fullPath;
    }

    private string GetRelativePath(string filename, string folder) {
        var normalizedFolder = folder.Replace('\\', '/').Trim('/');
        if (string.IsNullOrWhiteSpace(normalizedFolder) ||
            normalizedFolder.Split('/').Any(part => part is "." or "..")) {
            throw new ArgumentException("La carpeta de almacenamiento no es válida.", nameof(folder));
        }

        return $"/{_uploadPath}/{normalizedFolder}/{filename}";
    }
}