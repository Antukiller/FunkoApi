# 📄 doc/15 — Autenticación JWT

---

## 1. Configuración

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });
```

---

## 2. appsettings.json

```json
{
  "Jwt": {
    "Key": "clave-secreta-de-al-menos-32-caracteres",
    "Issuer": "FunkoApi",
    "Audience": "FunkoApi",
    "ExpireMinutes": 60
  }
}
```

---

## 3. Uso en controladores

```csharp
[Authorize(Policy = "RequireAdminRole")]
[HttpPost]
public async Task<ActionResult> Create(...) { ... }

[AllowAnonymous]
[HttpGet("{id}")]
public async Task<ActionResult> GetById(int id) { ... }
```

---

## 4. Políticas de autorización

```csharp
options.AddPolicy("RequireAdminRole", policy =>
    policy.RequireRole("Admin"));
```

---

## 5. Endpoints protegidos vs públicos

| Endpoint | Método | Auth |
|----------|--------|------|
| Listar funkos | GET | Público (AllowAnonymous) |
| Obtener funko | GET | Admin |
| Crear funko | POST | Admin |
| Actualizar funko | PUT | Admin |
| Eliminar funko | DELETE | Admin |
| Listar categorías | GET | Público |
| Obtener categoría | GET | Público |
| Crear categoría | POST | Admin |
| Actualizar categoría | PUT | Admin |
| Eliminar categoría | DELETE | Admin |
| Proveedores | GET/POST/PUT/DELETE | Admin (escritura) / Público (lectura) |