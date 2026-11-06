namespace FunkoApiV._2.Dto.Cassandra;

public record FunkoPorProveedorCategoriaDto
{
    public string ProveedorId { get; init; } = string.Empty;
    public string CategoriaId { get; init; } = string.Empty;
    public int FunkoId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string ProveedorNombre { get; init; } = string.Empty;
    public string CategoriaNombre { get; init; } = string.Empty;
}