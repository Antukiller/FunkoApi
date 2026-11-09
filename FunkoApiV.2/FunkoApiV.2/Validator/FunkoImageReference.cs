namespace FunkoApiV._2.Validator;

internal static class FunkoImageReference {
    public static bool IsValid(string? value) {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.StartsWith("/uploads/funkos/", StringComparison.OrdinalIgnoreCase)) return true;

        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
