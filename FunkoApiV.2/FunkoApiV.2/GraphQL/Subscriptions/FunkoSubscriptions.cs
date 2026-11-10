using FunkoApiV._2.GraphQL.Events;

namespace FunkoApiV._2.GraphQL.Subscriptions;

public class FunkoSubscriptions {
    [Subscribe]
    [Topic("onFunkoCreado")]
    public FunkoCreadoEvent OnFunkoCreado ([EventMessage] FunkoCreadoEvent message) => message;
    
    
    [Subscribe]
    [Topic("onFunkoActualizado")]
    public FunkoActualizadoEvent OnFunkoActualizado ([EventMessage] FunkoActualizadoEvent message) => message;
    
    
    [Subscribe]
    [Topic("onFunkoEliminado")]
    public FunkoEliminadoEvent OnFunkoEliminado ([EventMessage] FunkoEliminadoEvent message) => message;
    
    [Subscribe]
    [Topic("onStockBajo")]
    public FunkoStockBajoEvent OnStockBajo ([EventMessage] FunkoStockBajoEvent message) => message;
}