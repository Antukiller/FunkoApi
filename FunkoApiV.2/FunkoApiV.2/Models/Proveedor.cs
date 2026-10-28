namespace FunkoApiV._2.Models;


public record Proveedor {
    [YoutubeId] 
    public string Id { get; init; } = null!;
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
}
