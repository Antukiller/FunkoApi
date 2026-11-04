namespace FunkoApiV._2.Error;

public sealed record NotFound(string Message) : DomainError(Message) {
    public static NotFound FromId(string id, string resource = "Recurso") =>
        new($"{resource} con ID {id} no encontrado");
}

public sealed record ValidationError(string Message) : DomainError(Message);

public sealed record Conflict(string Message) : DomainError(Message);

public sealed record BusinessRuleError(string Message) : DomainError(Message);

public sealed record InternalError(string Message) : DomainError(Message);

public sealed record UnauthorizedError(string Message) : DomainError(Message);
