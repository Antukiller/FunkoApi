using System;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Dto;

public record FunkoResponseDto {
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Precio { get; init; }
    public int Stock { get; init; }
    public string? Imagen { get; set; }
    public string Categoria { get; init; } = string.Empty;
    public string CategoriaId { get; init; }
    
    public string ProveedorId { get; set; } 
    public Models.Proveedor Proveedor { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}