using System.ComponentModel.DataAnnotations;

namespace FunkoApiV._2.Dto.Proveedor;

public class ProveedorRequestDto {
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(20, ErrorMessage = "El nombre solo puede tener 20 caracteres")]
    [MinLength(2, ErrorMessage = "El nombre debe tener 2 caracteres al menos")]
    public string Nombre { get; init; } = string.Empty;
    [Required(ErrorMessage = "La direccion es obligatoria")]
    [MaxLength(200, ErrorMessage = "La direccion solo puede tener 200 caracteres")]
    [MinLength(2, ErrorMessage = "La direccion debe tener 2 caracteres al menos")]
    public string Direccion { get; init; } = string.Empty;
}