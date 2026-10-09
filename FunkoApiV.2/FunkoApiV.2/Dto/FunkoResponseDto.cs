namespace FunkoApiV._2.Dto;

public record FunkoResponseDto {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Precio { get; set; }
    public bool Stock { get; set; }
    public Categoria Categoria { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}   