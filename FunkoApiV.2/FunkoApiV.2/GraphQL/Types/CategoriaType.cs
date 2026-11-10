using FunkoApiV._2.Models;

namespace FunkoApiV._2.GraphQL.Types;

public class CategoriaType  : ObjectType<Categoria> {
    protected override void Configure(IObjectTypeDescriptor<Categoria> descriptor) {
        descriptor.Name("Categoria");
        descriptor.Description("Una categoría de la colección de Funko Pops.");
        
        descriptor.Field(c => c.Id)
            .Type<NonNullType<StringType>>()
            .Description("El identificador único de la categoría.");
        
        descriptor.Field(c => c.Nombre)
            .Type<NonNullType<StringType>>()
            .Description("El nombre de la categoría.");
        
        descriptor.Field(c => c.Descripcion)
            .Type<StringType>()
            .Description("La descripción de la categoría.");
    }
}