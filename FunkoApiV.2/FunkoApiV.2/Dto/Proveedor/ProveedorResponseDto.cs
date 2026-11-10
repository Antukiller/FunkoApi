namespace FunkoApiV._2.Dto.Proveedor;

/// <summary>Representación de lectura de un proveedor.</summary>
public record ProveedorResponseDto {
    /// <summary>ObjectId de MongoDB en representación hexadecimal.</summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>Nombre del proveedor.</summary>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Dirección del proveedor.</summary>
    public string Direccion { get; set; } = string.Empty;
    /// <summary>Fecha de creación UTC.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Fecha de última actualización UTC, si se actualizó.</summary>
    public DateTime? UpdatedAt { get; set; }
}