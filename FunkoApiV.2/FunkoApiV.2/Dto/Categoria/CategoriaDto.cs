using System.ComponentModel.DataAnnotations;

namespace FunkoApiV._2.Dto;

public record CategoriaDto(
    string Id,
    string Nombre,
    string? Descripcion,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CategoriaRequestDto {
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MinLength(3, ErrorMessage = "El nombre debe tener 3 caracteres al menos")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Nombre { get; init; } = string.Empty;

    [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
    public string? Descripcion { get; init; }
}

public record CategoriaFilterRequest {
    public string? Nombre { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "La página no puede ser negativa")]
    public int Page { get; init; } = 0;

    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
    public int Size { get; init; } = 10;

    public string SortBy { get; init; } = "id";
    public string Direction { get; init; } = "asc";
    public bool IncludeDeleted { get; init; } = false;
}
