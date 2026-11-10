namespace FunkoApiV._2.Dto.Common;

/// <summary>Formato común de los errores devueltos por la API.</summary>
public sealed record ApiErrorResponse {
    /// <summary>Descripción legible del error.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Errores de validación por campo; solo aparece si aplica.</summary>
    public IDictionary<string, string[]>? Errors { get; init; }
}
