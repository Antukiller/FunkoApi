using FunkoApiV._2.Models;

namespace FunkoApiV._2.GraphQL.Types;

public class FunkoType : ObjectType<Funko> {
    protected override void Configure(IObjectTypeDescriptor<Funko> descriptor) {
        descriptor.Name("Funko");
        descriptor.Description("Un Funko de la colección de Funko Pops.");
        
        descriptor.Field(f => f.Id)
            .Type<NonNullType<IdType>>()
            .Description("El identificador único del Funko.");
        
        descriptor.Field(f => f.Name)
            .Type<NonNullType<StringType>>()
            .Description("El nombre del Funko.");

        descriptor.Field(f => f.Precio)
            .Type<NonNullType<DecimalType>>()
            .Description("El precio del Funko.");
        
        descriptor.Field(f => f.Stock)
            .Type<NonNullType<IntType>>()
            .Description("La cantidad de Funkos disponibles en stock.");
        
        descriptor.Field(f => f.Imagen)
            .Type<StringType>()
            .Description("La URL de la imagen del Funko.");
        
        descriptor.Field(f => f.CategoriaId)
            .Type<NonNullType<IdType>>()
            .Description("El identificador de la categoría a la que pertenece el Funko.");
        
        descriptor.Field(f => f.ProveedorId)
            .Type<NonNullType<IdType>>()
            .Description("El identificador del proveedor del Funko.");
        
        descriptor.Field(f => f.IsDeleted)
            .Type<NonNullType<BooleanType>>()
            .Description("Indica si el Funko está eliminado.");
        
        descriptor.Field(f => f.CreatedAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("La fecha y hora en que se creó el Funko.");
        
        descriptor.Field(f => f.UpdatedAt)
            .Type<DateTimeType>()
            .Description("La fecha y hora en que se actualizó por última vez el Funko).");
        
        descriptor.Field(f => f.Categoria)
            .Type<NonNullType<CategoriaType>>()
            .Description("La categoría a la que pertenece el Funko.");

    }
}