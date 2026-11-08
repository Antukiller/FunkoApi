using System.ComponentModel.DataAnnotations;

namespace FunkoApiV._2.GraphQL.Inputs;

public record CreateFunkoInput {
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, 99999.99, ErrorMessage = "El precio debe estar entre 0.01 y 99999.99")]
    public decimal Precio { get; set; }
    
    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, 999999999, ErrorMessage = "El stock no puede ser negativo y no puede exceder 999999999")]
    public int Stock { get; set; }
    
    [Required(ErrorMessage = "La categoría es obligatoria")]
    public string CategoriaId { get; set; }
    
    [Required(ErrorMessage = "El proveedor es obligatorio")]
    public string ProveedorId { get; set; }
    
    public string? Imagen { get; set; }
}