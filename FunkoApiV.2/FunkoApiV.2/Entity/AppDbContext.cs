using FunkoApiV._2.Models;
using Microsoft.EntityFrameworkCore;

namespace FunkoApiV._2.Entity;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    public DbSet<Funko> Funkos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }


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
            e.Property(x => x.CategoriaId)
                .IsRequired();
            e.HasOne(f => f.Categoria)
                .WithMany(c => c.Funkos)
                .HasForeignKey(f => f.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
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

            e.Property(x => x.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(x => x.Descripcion)
                .HasMaxLength(500);
        });
    }
}