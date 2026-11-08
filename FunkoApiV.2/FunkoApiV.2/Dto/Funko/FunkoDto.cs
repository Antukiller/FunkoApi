using System.ComponentModel.DataAnnotations;

namespace FunkoApiV._2.Dto;

public record FunkoDto(
    long Id,
    string Name,
    decimal Precio,
    int Stock,
    string? Imagen,
    string Categoria,
    string CategoriaId,
    string ProveedorId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record FunkoResponseDto(
    long Id,
    string Name,
    decimal Precio,
    int Stock,
    string? Imagen,
    string Categoria,
    string CategoriaId,
    string ProveedorId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record FunkoCreateRequest {
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(20, ErrorMessage = "El nombre solo puede tener 20 caracteres")]
    [MinLength(2, ErrorMessage = "El nombre debe tener 2 caracteres al menos")]
    public string Name { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "50.0", ErrorMessage = "El precio debe estar entre 0.01 y 50.0")]
    public decimal Precio { get; init; }

    [Required(ErrorMessage = "El ID de la categoría es obligatorio")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "El ID de la categoría debe tener 11 caracteres")]
    public string CategoriaId { get; init; } = string.Empty;

    [Required(ErrorMessage = "El ID del proveedor es obligatorio")]
    [RegularExpression("^[a-fA-F0-9]{24}$", ErrorMessage = "El ID del proveedor debe ser un ObjectId de 24 caracteres hexadecimales")]
    public string ProveedorId { get; init; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; init; }

    [Required(ErrorMessage = "La imagen es obligatoria")]
    [MaxLength(255, ErrorMessage = "La ruta de la imagen no puede superar los 255 caracteres")]
    public string Imagen { get; init; } = string.Empty;
}

public record FunkoUpdateRequest {
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(20, ErrorMessage = "El nombre solo puede tener 20 caracteres")]
    [MinLength(2, ErrorMessage = "El nombre debe tener 2 caracteres al menos")]
    public string Name { get; init; } = string.Empty;

    [Range(0.01, 50.0, ErrorMessage = "El precio debe estar entre 0.01 y 50.0")]
    public decimal Precio { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; init; }

    [MaxLength(500, ErrorMessage = "La URL de la imagen no puede superar 500 caracteres")]
    public string? Imagen { get; init; }

    [Required(ErrorMessage = "El ID de la categoría es obligatorio")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "El ID de la categoría debe tener 11 caracteres")]
    public string CategoriaId { get; init; } = string.Empty;

    [Required(ErrorMessage = "El ID del proveedor es obligatorio")]
    [RegularExpression("^[0-9a-fA-F]{24}$", ErrorMessage = "El ID del proveedor debe ser un ObjectId de 24 caracteres hex")]
    public string ProveedorId { get; init; } = string.Empty;
}

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

public record ImagenResponseDto(string Imagen);
