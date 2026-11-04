using FunkoApiV._2.Entity;
using FunkoApiV._2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FunkoApiV._2.Seeders;

/// <summary>
/// Seeder explícito e idempotente para los datos iniciales de PostgreSQL
/// (categorías y funkos). Complementa a <c>HasData</c> de EF Core y se ejecuta
/// en el arranque de la aplicación.
/// </summary>
public static class DbSeeder {
    private const string ProveedorSeedId = "672f1a8b3c9d4e5f6a7b8c9d";

    public static async Task SeedAsync(AppDbContext db, ILogger logger) {
        if (!await db.Categorias.AnyAsync()) {
            var seedDate = DateTime.UtcNow;
            db.Categorias.AddRange(
                new Categoria { Id = "Anime000001", Nombre = "Anime", Descripcion = "Personajes de series de anime y manga", CreatedAt = seedDate, IsDeleted = false },
                new Categoria { Id = "Videojuego1", Nombre = "Videojuegos", Descripcion = "Personajes de consolas y PC", CreatedAt = seedDate, IsDeleted = false },
                new Categoria { Id = "Peliculas01", Nombre = "Películas", Descripcion = "Cine y series de televisión", CreatedAt = seedDate, IsDeleted = false }
            );
            await db.SaveChangesAsync();
            logger.LogInformation("Seed de categorías aplicado");
        }

        if (!await db.Funkos.AnyAsync()) {
            var seedDate = DateTime.UtcNow;
            db.Funkos.AddRange(
                new Funko { Name = "Kirito", Precio = 15.99m, Stock = 10, Imagen = "/uploads/funkos/placeholder.png", CategoriaId = "Anime000001", ProveedorId = ProveedorSeedId, CreatedAt = seedDate, IsDeleted = false },
                new Funko { Name = "Hinata Shoyo", Precio = 14.50m, Stock = 8, Imagen = "/uploads/funkos/placeholder.png", CategoriaId = "Anime000001", ProveedorId = ProveedorSeedId, CreatedAt = seedDate, IsDeleted = false },
                new Funko { Name = "Master Chief", Precio = 19.99m, Stock = 5, Imagen = "/uploads/funkos/placeholder.png", CategoriaId = "Videojuego1", ProveedorId = ProveedorSeedId, CreatedAt = seedDate, IsDeleted = false },
                new Funko { Name = "Geralt de Rivia", Precio = 17.50m, Stock = 12, Imagen = "/uploads/funkos/placeholder.png", CategoriaId = "Videojuego1", ProveedorId = ProveedorSeedId, CreatedAt = seedDate, IsDeleted = false },
                new Funko { Name = "Darth Vader", Precio = 21.00m, Stock = 3, Imagen = "/uploads/funkos/placeholder.png", CategoriaId = "Peliculas01", ProveedorId = ProveedorSeedId, CreatedAt = seedDate, IsDeleted = false }
            );
            await db.SaveChangesAsync();
            logger.LogInformation("Seed de funkos aplicado");
        }
    }
}
