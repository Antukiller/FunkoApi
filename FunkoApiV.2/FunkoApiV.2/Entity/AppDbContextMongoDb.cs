using FunkoApiV._2.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace FunkoApiV._2.Entity;

public class AppDbContextMongoDb(DbContextOptions<AppDbContextMongoDb>  options) : DbContext(options) {
    public DbSet<Proveedor> Proveedor =>  Set<Proveedor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Proveedor>().ToCollection("proveedor"); 
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }
}