namespace FunkoApiV._2.Dto.Cassandra;

public record FunkosPorCategoriaDto {
    public string CategoriaId { get; init; } = string.Empty;
    public int FunkoId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public int Stock { get; init; }
    public decimal Precio { get; init; }
    public string CategoriaNombre { get; init; } = string.Empty;
}