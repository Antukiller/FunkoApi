using FunkoApiV._2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FunkoApiV._2.Entity;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, IdentityRole<long>, long>(options) {
    public DbSet<Funko> Funkos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }

    private const string ProveedorSeedId = "672f1a8b3c9d4e5f6a7b8c9d";

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
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

            e.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
            e.HasQueryFilter(c => !c.IsDeleted);
            
        });
    }
}
