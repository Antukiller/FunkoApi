namespace FunkoApiV._2.Dto.Common;

/// <summary>
/// Envoltorio común para todas las respuestas paginadas de la API.
/// La página inicial es 0, igual que en los filtros del profesor.
/// </summary>
public record PagedResult<T> {
    /// <summary>Elementos de la página solicitada.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];
    /// <summary>Total de elementos coincidentes en todas las páginas.</summary>
    public int TotalCount { get; init; }
    /// <summary>Índice base cero de la página devuelta.</summary>
    public int Page { get; init; }
    /// <summary>Número máximo de elementos por página.</summary>
    public int PageSize { get; init; }

    /// <summary>Número total de páginas.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    /// <summary>Indica si existe una página posterior.</summary>
    public bool HasNextPage => Page + 1 < TotalPages;
    /// <summary>Indica si existe una página anterior.</summary>
    public bool HasPreviousPage => Page > 0;
}
