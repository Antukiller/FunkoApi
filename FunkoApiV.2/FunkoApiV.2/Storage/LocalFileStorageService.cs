using FunkoApiV._2.Infrastructure.Interfaces;

namespace FunkoApiV._2.Storage;

public class LocalFileStorageService(IWebHostEnvironment environment) : IFileStorageService, ITransientService {
    private const long MaxSize = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    public async Task<string> SaveAsync(IFormFile file, string folder) {
        if (file is null || file.Length == 0)
            throw new ArgumentException("El fichero está vacío");

        if (file.Length > MaxSize)
            throw new ArgumentException("El fichero no puede superar los 5 MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Formato no permitido. Usa jpg, jpeg, png, webp o gif");

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var absoluteFolder = Path.Combine(webRoot, "uploads", folder);
        Directory.CreateDirectory(absoluteFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(absoluteFolder, fileName);

        await using var stream = File.Create(absolutePath);
        await file.CopyToAsync(stream);

        return $"/uploads/{folder}/{fileName}";
    }

    public void Delete(string? relativePath) {
        if (string.IsNullOrWhiteSpace(relativePath) ||
            !relativePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            return;

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var absolutePath = Path.Combine(
            webRoot,
            relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
    }
}
