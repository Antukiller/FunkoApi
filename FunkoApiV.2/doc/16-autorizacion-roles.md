# 📄 doc/16 — Autorización por Roles

---

## 1. Rol Admin

Solo usuarios con rol `Admin` pueden realizar operaciones de escritura (POST, PUT, DELETE).

```csharp
[Authorize(Policy = "RequireAdminRole")]
[HttpPost]
public async Task<ActionResult> Create(...) { ... }
```

---

## 2. Política

```csharp
options.AddPolicy("RequireAdminRole", policy =>
    policy.RequireRole("Admin"));
```

---

## 3. Matriz de permisos

| Acción | Rol Admin | Usuario autenticado | Anónimo |
|--------|-----------|---------------------|---------|
| Leer funkos | ✅ | ✅ | ✅ |
| Crear funko | ✅ | ❌ | ❌ |
| Actualizar funko | ✅ | ❌ | ❌ |
| Eliminar funko | ✅ | ❌ | ❌ |
| Leer categorías | ✅ | ✅ | ✅ |
| Crear categoría | ✅ | ❌ | ❌ |
| Eliminar categoría | ✅ | ❌ | ❌ |
| Leer proveedores | ✅ | ✅ | ✅ |
| Gestionar proveedores | ✅ | ❌ | ❌ |

---

## 4. Manejo de 403

Cuando un usuario autenticado no tiene el rol requerido:

```csharp
ForbiddenError e => new ObjectResult(new { message = e.Message }) 
    { StatusCode = StatusCodes.Status403Forbidden }
```