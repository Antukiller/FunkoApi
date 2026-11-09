# 📄 doc/08 — EF Core con PostgreSQL

---

## 1. DbContext

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) 
    : IdentityDbContext<AppUser, IdentityRole<long>, long>
{
    public DbSet<Funko> Funkos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Funko>(e => {
            e.ToTable("funko");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(20);
            e.Property(x => x.Precio).IsRequired().HasPrecision(20, 2);
            e.Property(x => x.Imagen).HasMaxLength(255);
            e.Property(x => x.CategoriaId).IsRequired().HasMaxLength(11);
            e.Property(x => x.ProveedorId).IsRequired().HasMaxLength(24);
            e.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
            e.HasQueryFilter(f => !f.IsDeleted); // Soft delete global
        });

        modelBuilder.Entity<Categoria>(e => {
            e.ToTable("categoria");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(11); // ID tipo YouTube
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
            e.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
            e.HasQueryFilter(c => !c.IsDeleted);
        });
    }
}
```

---

## 2. Configuración de conexión

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(FunkoConfig.PostgresConnection));
```

---

## 3. Migraciones

```powershell
dotnet ef migrations add InitialCreate --project FunkoApiV.2
dotnet ef database update --project FunkoApiV.2
```

---

## 4. Seed de datos

Se siembran datos iniciales en `OnModelCreating` mediante `HasData`:

```csharp
e.HasData(
    new Funko { Id = 1, Name = "Kirito", Precio = 15.99m, Stock = 10, ... },
    new Categoria { Id = "Anime000001", Nombre = "Anime", ... }
);
```

---

## 5. Reglas específicas

| Regla | Implementación |
|-------|---------------|
| Tabla en snake_case | `e.ToTable("funko")`, `e.ToTable("categoria")` |
| ID categoría YouTube | `HasMaxLength(11)` + generador personalizado |
| Soft delete | `HasQueryFilter(f => !f.IsDeleted)` |
| Precisión decimal | `HasPrecision(20, 2)` para precios |
| Relación Funko → Categoría | Foreign key `CategoriaId`, `OnDelete(DeleteBehavior.Restrict)` |