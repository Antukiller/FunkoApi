using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FunkoApiV._2.Models;
public record Proveedor {
    [BsonId] public ObjectId Id { get; init; } = ObjectId.GenerateNewId();
    [BsonElement("nombre")] 
    public string Nombre { get; set; } = string.Empty;
    [BsonElement("direccion")]
    public string Direccion { get; set; } = string.Empty;
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; }
    [BsonElement("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    [BsonElement("id_deleted")]
    public bool IsDeleted { get; set; }
}
