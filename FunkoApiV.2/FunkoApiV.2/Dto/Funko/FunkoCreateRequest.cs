using System.ComponentModel.DataAnnotations;

namespace FunkoApiV._2.Dto;

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
