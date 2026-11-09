using FunkoApiV._2.Entity;
using FunkoApiV._2.Models;
using Microsoft.EntityFrameworkCore;

namespace FunkoApiV._2.DataLoaders;

/// <summary>
/// DataLoader para cargar categorias por ID.
/// Resuelve el problema N+1 agrupando las consultas.
/// </summary>
public class CategoriaByIdDataLoader : BatchDataLoader<string, Categoria>
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public CategoriaByIdDataLoader(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options)
        : base(batchScheduler, options)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task<IReadOnlyDictionary<string, Categoria>> LoadBatchAsync(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Categorias
            .Where(c => keys.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);
    }
}