# 📄 doc/09 — MongoDB (Proveedores)

> ⚠️ **Nota**: Este módulo es responsabilidad del compañero. Se documenta aquí para referencia de la arquitectura completa.

---

## 1. Entidad Proveedor

```csharp
public class Proveedor
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
```

---

## 2. DbContext MongoDB

```csharp
public class AppDbContextMongoDb(DbContextOptions<AppDbContextMongoDb> options) 
    : DbContext(options)
{
    public DbSet<Proveedor> Proveedor => Set<Proveedor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proveedor>().ToCollection("proveedor");
    }
}
```

---

## 3. Configuración

```csharp
builder.Services.AddDbContext<AppDbContextMongoDb>(options =>
    options.UseMongoDB(connectionString, databaseName));
```

---

## 4. Relación con Funko (cruz BD)

- `Funko.ProveedorId` almacena el ObjectId del proveedor como string (24 chars)
- El service valida que el proveedor existe consultando el repositorio de MongoDB
- Es una **relación lógica cruzada** entre PostgreSQL y MongoDB

---

## 5. Operaciones CRUD

| Operación | Repositorio | Service |
|-----------|-------------|---------|
| Listar | `GetPagedAsync` | `GetAll` |
| Obtener por ID | `GetById` | `GetById` |
| Crear | `CreateAsync` | `Add` |
| Actualizar | `UpdateAsync` | `Update` |
| Eliminar | `DeleteAsync` | `Delete` |

---

## 6. Consideraciones

- No hay transacciones distribuidas entre PostgreSQL y MongoDB
- Si falla la creación en uno, no se hace rollback en el otro
- El ID de proveedor se valida con regex `^[a-fA-F0-9]{24}$`