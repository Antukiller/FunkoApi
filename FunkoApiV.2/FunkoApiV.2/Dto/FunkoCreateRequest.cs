using System.ComponentModel.DataAnnotations;

namespace FunkoApiV._2.Dto;

public record FunkoCreateRequest {
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(20, ErrorMessage = "El nombre solo puede tener 20 caracteres")]
    [MinLength(2, ErrorMessage = "El nombre debe tener 2 caracteres al menos")]
    public string Name { get; init; } = string.Empty;
    
    [Range(0.01, 50.0, ErrorMessage = "El precio debe estar entre 0.01 y 50.0")]
    public double Precio { get; init; }
    
    [Required(ErrorMessage = "La categoría es obligatoria")]
    [MaxLength(20, ErrorMessage = "La categoria solo puede tener 20 caracteres")]
    [MinLength(2, ErrorMessage = "La categoria debe tener 2 caracteres al menos")]
    public string Categoria { get; init; } = string.Empty;
    
    [Required(ErrorMessage = "El campo activo es obligatorio")]
    public bool Stock { get; init; }
}