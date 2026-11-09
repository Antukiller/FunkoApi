using FunkoApiV._2.Dto;
using FunkoApiV._2.GraphQL.Events;
using FunkoApiV._2.GraphQL.Inputs;
using FunkoApiV._2.Service;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using HotChocolate.Types;

namespace FunkoApiV._2.GraphQL.Mutation;

public class FunkoMutation {
    [Authorize(Policy = "RequireAdmin")]
    public async Task<FunkoResponseDto?> CrearFunko(
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
        
        var result = await service.Add(dto);
        if (result.IsFailure) throw new GraphQLException(result.Error.Message);
        
        _ = eventSender.SendAsync("onFunkoCreado", new FunkoCreadoEvent {
            FunkoId = result.Value.Id,
            Name = result.Value.Name,
            Precio = result.Value.Precio,
            Stock = result.Value.Stock,
            CategoriaId = result.Value.CategoriaId,
            ProveedorId = result.Value.ProveedorId,
            CreatedAt = result.Value.CreatedAt
        });
        return result.Value;
        
        }
    
    [Authorize(Policy = "RequireAdmin")]
    public async Task<FunkoResponseDto?> ActualizarFunko(
        long id,
        UpdateFunkoInput input,
        [Service] IFunkoService service,
        [Service] ITopicEventSender eventSender) {
        
        var dto = new FunkoUpdateRequest() {
            Name = input.Name,
            Precio = input.Precio,
            Stock = input.Stock,
            Imagen = input.Imagen,
            CategoriaId = input.CategoriaId,
            ProveedorId = input.ProveedorId
        };
        
        var result = await service.Update(id, dto);
        if (result.IsFailure) throw new GraphQLException(result.Error.Message);
        
        _ = eventSender.SendAsync("onFunkoActualizado", new FunkoActualizadoEvent {
            FunkoId = result.Value.Id,
            Name = result.Value.Name,
            Precio = result.Value.Precio,
            Stock = result.Value.Stock,
            CategoriaId = result.Value.CategoriaId,
            UpdatedAt = result.Value.UpdatedAt ?? DateTime.UtcNow
        });
        return result.Value;
        
    }
    
    
    [Authorize(Policy = "RequireAdmin")]
    public async Task<bool> EliminarFunko(
    long id,
    [Service] IFunkoService service,
    [Service] ITopicEventSender eventSender) {
    
    var result = await service.Delete(id);
    if (result.IsSuccess) {
        _ = eventSender.SendAsync("onFunkoEliminado", new FunkoEliminadoEvent {
            FunkoId = id,
            DeletedAt = DateTime.UtcNow
        });
    }
    return result.IsSuccess;
        
    }
    
    [Authorize(Policy = "RequireAdmin")]
    public async Task<FunkoResponseDto?> ActualizarImagenFunko(
        long id,
        [Service] IFunkoService service,
        [Service] ITopicEventSender eventSender,
        IFile file) {
        var length = file.Length
            ?? throw new GraphQLException("No se pudo determinar el tamaño del archivo.");
        await using var stream = file.OpenReadStream();
        var formFile = new FormFile(stream, 0, length, "file", file.Name) {
            Headers = new HeaderDictionary(),
            ContentType = file.ContentType ?? "application/octet-stream"
        };

        var result = await service.UpdateImagen(id, formFile);
        if (result.IsFailure) return null;
        _ = eventSender.SendAsync("FunkoImagenActualizada", result.Value);
        return result.Value;
        
    }
    


}