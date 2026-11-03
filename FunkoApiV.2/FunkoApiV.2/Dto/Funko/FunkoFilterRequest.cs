using System.ComponentModel.DataAnnotations;

namespace FunkoApiV._2.Dto;

/// <summary>
/// Filtros y paginación del listado de funkos.
/// </summary>
public record FunkoFilterRequest {
    public string? Nombre { get; init; }
    public string? CategoriaId { get; init; }
    public string? ProveedorId { get; init; }
    public decimal? PrecioMin { get; init; }
    public decimal? PrecioMax { get; init; }
    public int? StockMin { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "La página no puede ser negativa")]
    public int Page { get; init; } = 0;

    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
    public int Size { get; init; } = 10;

    public string SortBy { get; init; } = "id";
    public string Direction { get; init; } = "asc";
    public bool IncludeDeleted { get; init; } = false;
}
