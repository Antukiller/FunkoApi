namespace FunkoApiV._2.Dto.Common;

/// <summary>
/// Envoltorio común para todas las respuestas paginadas de la API.
/// La página inicial es 0, igual que en los filtros del profesor.
/// </summary>
public record PagedResult<T> {
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => Page + 1 < TotalPages;
    public bool HasPreviousPage => Page > 0;
}
