namespace FunkoApiV._2.Storage;

/// <summary>
/// Guarda ficheros en el servidor y devuelve siempre la ruta relativa
/// (por ejemplo /uploads/funkos/abc.png), que es lo único que se persiste en la BD.
/// </summary>
public interface IFileStorageService {
    Task<string> SaveAsync(IFormFile file, string folder);

    void Delete(string? relativePath);
}
