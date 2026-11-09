# 📄 doc/17 — Seguridad HTTP

---

## 1. Middleware de encabezados de seguridad

```csharp
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
        await next(context);
    }
}
```

---

## 2. Pipeline de seguridad

| Middleware | Cuándo |
|------------|--------|
| `UseGlobalExceptionHandler()` | Siempre, primero |
| `UseSecurityHeaders()` | Siempre |
| `UseRateLimiting()` | Siempre |
| `UseHsts()` | Solo producción |
| `UseHttpsRedirection()` | Solo producción |
| `UseCorsPolicy()` | Siempre |
| `UseAuthentication()` | Siempre |
| `UseAuthorization()` | Siempre |

---

## 3. CORS

```csharp
services.AddCorsPolicy(builder.Configuration);
// Con WithOrigins, nunca AllowAnyOrigin en producción
```

---

## 4. Rate Limiting

Límite por IP y por endpoint para prevenir abuso.

---

## 5. Headers de respuesta

| Header | Valor | Propósito |
|--------|-------|-----------|
| X-Content-Type-Options | nosniff | Evitar MIME sniffing |
| X-Frame-Options | DENY | Prevenir clickjacking |
| Referrer-Policy | no-referrer | No enviar referer |
| Content-Security-Policy | default-src 'self' | Restringir recursos |