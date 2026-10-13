using System;
using FunkoApiV._2.Models;

namespace FunkoApiV._2.Dto;

public record FunkoResponseDto {
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Precio { get; init; }
    public bool Stock { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}   