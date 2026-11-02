namespace FunkoApiV._2.Dto.Proveedor;

public record ProveedorResponseDto {
    public string Id { get; init; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}