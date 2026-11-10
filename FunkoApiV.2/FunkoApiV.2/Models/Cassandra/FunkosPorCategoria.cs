using Cassandra.Mapping.Attributes;

namespace FunkoApiV._2.Models.Cassandra;

[Table("funkos_por_categorias")]
public class FunkosPorCategoria {
    [Column("categoria_id")]
    [PartitionKey]
    public string CategoriaId { get; set; }
    
    [Column("funkos_id")]
    [ClusteringKey]
    public long FunkosId { get; set; }
    
    [Column("nombre")]
    public string Nombre { get; set; }
    [Column("precio")]
    public decimal Precio { get; set; }
    
    [Column("stock")]
    public int Stock { get; set; }
    
    [Column("categoria_nombre")]
    public string CategoriaNombre { get; set; }
}