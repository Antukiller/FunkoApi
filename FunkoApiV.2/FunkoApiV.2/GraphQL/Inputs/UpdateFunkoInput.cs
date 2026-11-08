using System.ComponentModel.DataAnnotations;

namespace FunkoApiV._2.GraphQL.Inputs;

public record UpdateFunkoInput {
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Range(0.01, 99999.99, ErrorMessage = "El precio debe estar entre 0.01 y 99999.99")]
    public decimal Precio { get; set; }
    
    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, 999999, ErrorMessage = "El stock no puede ser negativo y no puede superar 999999")] 
    public int Stock { get; set; }
    
    [Required(ErrorMessage = "La categoría es obligatorio")]
    public long CategoriaId { get; set; }
    
    public string? Imagen { get; set; }
}