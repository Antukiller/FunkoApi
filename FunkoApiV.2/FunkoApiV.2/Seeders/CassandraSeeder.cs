using Cassandra;
using FunkoApiV._2.Entity;
using FunkoApiV._2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ISession = Cassandra.ISession;

namespace FunkoApiV._2.Seeders;

/// <summary>
/// Seeder del read model de Cassandra.
/// No inventa datos: los reconstruye desde Postgres (Funkos + Categorías)
/// y Mongo (Proveedores), que son la fuente de verdad.
/// </summary>
public static class CassandraSeeder {
    public static async Task SeedAsync(
        ISession session, AppDbContext db, AppDbContextMongoDb dbMongo, ILogger logger) {

        // ── 1. Funkos activos con su categoría (Postgres) ──
        var funkos = await db.Funkos
            .Where(f => !f.IsDeleted)
            .Include(f => f.Categoria)
            .ToListAsync();

        if (funkos.Count == 0) {
            logger.LogWarning("CassandraSeeder: no hay funkos en Postgres, no se siembra Cassandra");
            return;
        }

        // ── 2. Nombres de proveedor (Mongo) ──
        var proveedores = await dbMongo.Proveedor.ToListAsync();
        var nombresProveedor = proveedores.ToDictionary(p => p.Id.ToString(), p => p.Nombre);

        // ── 3. Reconstruir las 3 tablas (idempotente) ──
        await session.ExecuteAsync(new SimpleStatement("TRUNCATE funkos_por_categorias"));
        await session.ExecuteAsync(new SimpleStatement("TRUNCATE funkos_por_proveedor"));
        await session.ExecuteAsync(new SimpleStatement("TRUNCATE funkos_por_proveedor_categoria"));

        // ── 4. Cada funko se escribe en las 3 vistas ──
        foreach (var funko in funkos) {
            var categoriaNombre = funko.Categoria?.Nombre ?? string.Empty;
            var proveedorNombre = nombresProveedor.GetValueOrDefault(funko.ProveedorId, string.Empty);

            await session.ExecuteAsync(new SimpleStatement(
                "INSERT INTO funkos_por_categorias " +
                "(categoria_id, funkos_id, nombre, stock, precio, categoria_nombre) " +
                "VALUES (?, ?, ?, ?, ?, ?)",
                funko.CategoriaId, funko.Id, funko.Name, funko.Stock, funko.Precio, categoriaNombre));

            await session.ExecuteAsync(new SimpleStatement(
                "INSERT INTO funkos_por_proveedor " +
                "(proveedor_id, funkos_id, nombre, stock, precio, proveedor_nombre) " +
                "VALUES (?, ?, ?, ?, ?, ?)",
                funko.ProveedorId, funko.Id, funko.Name, funko.Stock, funko.Precio, proveedorNombre));

            await session.ExecuteAsync(new SimpleStatement(
                "INSERT INTO funkos_por_proveedor_categoria " +
                "(proveedor_id, categoria_id, funkos_id, nombre, proveedor_nombre, categoria_nombre) " +
                "VALUES (?, ?, ?, ?, ?, ?)",
                funko.ProveedorId, funko.CategoriaId, funko.Id,
                funko.Name, proveedorNombre, categoriaNombre));
        }

        logger.LogInformation("Seed de Cassandra aplicado: {Count} funkos en 3 tablas", funkos.Count);
    }
}