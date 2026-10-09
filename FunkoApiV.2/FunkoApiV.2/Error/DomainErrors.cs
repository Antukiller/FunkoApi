namespace FunkoApiV._2.Error;

public sealed record NotFound(string Message) : DomainError(Message) {
    public static NotFound FromId(int id, string resource = "Recurso") =>
        new($"{resource} con ID {id} no encontrado");
}
public sealed record ValidationError(string Message, Dictionary<string, string[]>? Errors = null) : DomainError(Message) {
    public static ValidationError Create(string message) =>
        new(message);
    public static ValidationError WithFieldErrors(Dictionary<string, string[]> errors) =>
        new("Errores de validación", errors);
}