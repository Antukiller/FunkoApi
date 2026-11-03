using FunkoApiV._2.Models;
using Microsoft.EntityFrameworkCore;

namespace FunkoApiV._2.Entity;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    public DbSet<Funko> Funkos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }

    private const string ProveedorSeedId = "672f1a8b3c9d4e5f6a7b8c9d";

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Funko>(e => {
            e.ToTable("funko");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(20);
            e.Property(x => x.Precio)
                .IsRequired()
                .HasPrecision(20, 2);
            e.Property(x => x.Stock)
                .IsRequired();
            e.Property(x => x.Imagen)
                .HasMaxLength(255);
            e.Property(x => x.CategoriaId)
                .IsRequired()
                .HasMaxLength(11);
            e.HasOne(f => f.Categoria)
                .WithMany()
                .HasForeignKey(f => f.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.ProveedorId)
                .IsRequired()
                .HasMaxLength(24);
            e.Property(x => x.CreatedAt)
                .IsRequired();
            e.Property(x => x.UpdatedAt)
                .IsRequired(false);
            e.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
            e.HasQueryFilter(f => !f.IsDeleted);
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            e.HasData(
                new Funko { Id = 1, Name = "Kirito", Precio = 15.99m, Stock = 10, Imagen = "/uploads/funkos/placeholder.png", CategoriaId = "Anime000001", ProveedorId = ProveedorSeedId, CreatedAt = seedDate, IsDeleted = false },
                new Funko { Id = 2, Name = "Hinata Shoyo", Precio = 14.50m, Stock = 8, Imagen = "/uploads/funkos/placeholder.png", CategoriaId = "Anime000001", ProveedorId = ProveedorSeedId, CreatedAt = seedDate, IsDeleted = false },
                new Funko { Id = 3, Name = "Master Chief", Precio = 19.99m, Stock = 5, Imagen = "/uploads/funkos/placeholder.png", CategoriaId = "Videojuego1", ProveedorId = ProveedorSeedId, CreatedAt = seedDate, IsDeleted = false },
                new Funko { Id = 4, Name = "Geralt de Rivia", Precio = 17.50m, Stock = 12, Imagen = "/uploads/funkos/placeholder.png", CategoriaId = "Videojuego1", ProveedorId = ProveedorSeedId, CreatedAt = seedDate, IsDeleted = false },
                new Funko { Id = 5, Name = "Darth Vader", Precio = 21.00m, Stock = 3, Imagen = "/uploads/funkos/placeholder.png", CategoriaId = "Peliculas01", ProveedorId = ProveedorSeedId, CreatedAt = seedDate, IsDeleted = false }
            );
        });
        modelBuilder.Entity<Categoria>(e => {
            e.ToTable("categoria");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id)
                .HasMaxLength(11);

            e.Property(x => x.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(x => x.Descripcion)
                .HasMaxLength(500);
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            e.HasData(
                new Categoria { Id = "Anime000001", Nombre = "Anime", Descripcion = "Personajes de series de anime y manga", CreatedAt = seedDate, IsDeleted = false },
                new Categoria { Id = "Videojuego1", Nombre = "Videojuegos", Descripcion = "Personajes de consolas y PC", CreatedAt = seedDate, IsDeleted = false },
                new Categoria { Id = "Peliculas01", Nombre = "Películas", Descripcion = "Cine y series de televisión", CreatedAt = seedDate, IsDeleted = false }
            );
        });
    }
}
