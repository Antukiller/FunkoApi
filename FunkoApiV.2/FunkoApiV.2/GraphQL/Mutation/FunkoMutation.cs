using FunkoApiV._2.Dto;
using FunkoApiV._2.GraphQL.Inputs;
using FunkoApiV._2.Models;
using FunkoApiV._2.Service;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;

namespace FunkoApiV._2.GraphQL.Mutation;

public class FunkoMutation {
    [Authorize(Policy = "RequireAdmin")]
    public Funko? CrearFunko(
        CreateFunkoInput input,
        [Service] IFunkoService service,
        [Service] ITopicEventSender eventSender) {
        
        var dto = new FunkoCreateRequest() {
            Name = input.Name,
            Precio = input.Precio,
            Stock = input.Stock,
            Imagen = input.Imagen,
            CategoriaId = input.CategoriaId,
            ProveedorId = input.ProveedorId
        };
        
        var result = service.Add(dto);
        if (result.IsFaulted) return null;
        
        _ = eventSender.SendAsync("FunkoCreado", result.Result);
        return result.Result;
        
        }
    
    
    
    
    
}