namespace FunkoApiV._2.Error;

public static class StorageError {
    public static DomainError ArchivoVacio() => new BusinessRuleError("Debe contener al menos una imagen");
    public static DomainError ArchivoGrande() => new BusinessRuleError("La imagen es demasiado grande, Maxigo 5MB");
    public static DomainError ExtensionNoPermitida() => new BusinessRuleError("La extension no esta permitida. Use: .jpg, jpeg, png, .webp");
    public static DomainError TipoContenidoNoPermitido() => new BusinessRuleError("Tipo de contenido no permitido, solo imagenes");
    public static DomainError NombreArchivoInvalido() => new BusinessRuleError("La imagen es demasiado grande, Maxigo 5MB");
}