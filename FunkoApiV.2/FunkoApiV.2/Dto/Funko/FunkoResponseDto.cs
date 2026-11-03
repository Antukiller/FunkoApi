namespace FunkoApiV._2.Dto;

public record FunkoResponseDto {
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Precio { get; init; }
    public int Stock { get; init; }
    public string? Imagen { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public string CategoriaId { get; init; } = string.Empty;
    public string ProveedorId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record ImagenResponseDto(string Imagen);
