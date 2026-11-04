using CSharpFunctionalExtensions;
using FunkoApiV._2.Error;

namespace FunkoApiV._2.Storage;

/// <summary>Abstrae las operaciones de persistencia de imágenes.</summary>
public interface IStorageService {
    /// <summary>Guarda la imagen y devuelve su ruta relativa pública.</summary>
    Task<Result<string, DomainError>> SaveFileAsync(IFormFile file, string folder);

    /// <summary>Elimina una imagen previamente guardada.</summary>
    Task<Result<bool, DomainError>> DeleteFileAsync(string relativePath);
}