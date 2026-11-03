namespace FunkoApiV._2.Error;

public static class StorageError {
    public static DomainError ArchivoVacio() =>
        new BusinessRuleError("El archivo está vacío o no se ha proporcionado.");

    public static DomainError ArchivoMuyGrande() =>
        new BusinessRuleError("El archivo supera el tamaño máximo permitido.");

    public static DomainError ExtensionNoPermitida() =>
        new BusinessRuleError("La extensión del archivo no está permitida.");

    public static DomainError TipoContenidoNoPermitido() =>
        new BusinessRuleError("El tipo de contenido no está permitido.");

    public static DomainError NombreArchivoInvalido() =>
        new BusinessRuleError("El nombre del archivo no es válido.");

    public static DomainError ErrorGuardando() =>
        new InternalError("Error al guardar el archivo en el servidor.");

    public static DomainError ErrorEliminando() =>
        new InternalError("Error al eliminar el archivo del servidor.");
}