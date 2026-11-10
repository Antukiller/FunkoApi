using System.ComponentModel.DataAnnotations;

namespace FunkoApiV._2.Dto;

/// <summary>Representación de lectura de una categoría.</summary>
/// <param name="Id">Identificador de 11 caracteres.</param>
/// <param name="Nombre">Nombre de la categoría.</param>
/// <param name="Descripcion">Descripción opcional.</param>
/// <param name="CreatedAt">Fecha de creación UTC.</param>
/// <param name="UpdatedAt">Fecha de última actualización UTC, si se actualizó.</param>
public record CategoriaDto(
    string Id,
    string Nombre,
    string? Descripcion,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>Entrada para crear o actualizar una categoría.</summary>
public record CategoriaRequestDto {
    /// <summary>Nombre requerido entre 3 y 100 caracteres.</summary>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MinLength(3, ErrorMessage = "El nombre debe tener 3 caracteres al menos")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Nombre { get; init; } = string.Empty;

    /// <summary>Descripción opcional de hasta 500 caracteres.</summary>
    [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
    public string? Descripcion { get; init; }
}

/// <summary>Filtro, ordenación y paginación del listado de categorías.</summary>
public record CategoriaFilterRequest {
    /// <summary>Búsqueda parcial del nombre (sin distinguir mayúsculas/minúsculas).</summary>
    public string? Nombre { get; init; }

    /// <summary>Página base cero.</summary>
    [Range(0, int.MaxValue, ErrorMessage = "La página no puede ser negativa")]
    public int Page { get; init; } = 0;

    /// <summary>Elementos por página, entre 1 y 100.</summary>
    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
    public int Size { get; init; } = 10;

    /// <summary>Orden: id, nombre/name o createdat/created_at.</summary>
    public string SortBy { get; init; } = "id";
    /// <summary>Dirección asc o desc.</summary>
    public string Direction { get; init; } = "asc";
    /// <summary>Incluye categorías eliminadas lógicamente. Actualmente no requiere ADMIN.</summary>
    public bool IncludeDeleted { get; init; } = false;
}
