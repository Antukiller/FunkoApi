using Cassandra.Mapping.Attributes;

namespace FunkoApiV._2.Models.Cassandra;

[Table("funkos_por_proveedor_categoria")]
public class FunkoPorProveedorCategoria {
    [Column("proveedor_id")]
    [PartitionKey]
    public string ProveedorId { get; set; }
    [Column("categoria_id")]
    [PartitionKey]
    public string CategoriaId { get; set; }
    
    [Column("funkos_id")]
    [ClusteringKey]
    public long FunkosId { get; set; }
    
    [Column("nombre")]
    public string Nombre { get; set; }
    
    [Column("proveedor_nombre")]
    public string ProveedorNombre { get; set; }
    
    [Column("categoria_nombre")]
    public string CategoriaNombre { get; set; }
}