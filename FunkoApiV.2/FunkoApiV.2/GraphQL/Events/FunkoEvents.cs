namespace FunkoApiV._2.GraphQL.Events;


public record FunkoCreadoEvent {
    public long FunkoId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Precio { get; init; }
    public int Stock { get; init; }
    public string CategoriaId { get; init; }
    public string ProveedorId { get; init; }
    public DateTime CreatedAt { get; init; }
}


public record FunkoActualizadoEvent {
    public long FunkoId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Precio { get; init; }
    public int Stock { get; init; }
    public string CategoriaId { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record FunkoEliminadoEvent {
    public long FunkoId { get; init; }
    public DateTime DeletedAt { get; init; }
}

public record FunkoStockBajoEvent {
    public long FunkoId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int StockActual { get; init; }
    public int StockUmbral { get; init; }
    public DateTime DetectedAt { get; init; }
}



