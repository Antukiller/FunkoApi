using Cassandra;
using ISession = Cassandra.ISession;

namespace FunkoApiV._2.Infrastructure.Cassandra;

public static class CassandraSchemaExtensions {
    public static async Task InitializeSchemaAsync(this ISession session, string keyspace) {
        for (var i = 1; ; i++) {
            try {
                await session.ExecuteAsync(new SimpleStatement("SELECT now() FROM system.local"));
                break;
            } catch (NoHostAvailableException) when (i < 50) {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
        await session.ExecuteAsync(new SimpleStatement(
            $"CREATE KEYSPACE IF NOT EXISTS {keyspace} " + 
            "WITH replication = {'class': 'SimpleStrategy', 'replication_factor': '1'};"
        ));
        session.ChangeKeyspace(keyspace);

        await session.ExecuteAsync(new SimpleStatement("""
                                                        CREATE TABLE IF NOT EXISTS funkos_por_categorias (
                                                       categoria_id text,
                                                       funkos_id int,
                                                       nombre text,
                                                       stock int,
                                                       precio decimal,
                                                       categoria_nombre text,
                                                       PRIMARY KEY(categoria_id, funkos_id)
                                                       )
                                                       """));
        await session.ExecuteAsync(new SimpleStatement("""
                                                        CREATE TABLE IF NOT EXISTS funkos_por_proveedor (
                                                       proveedor_id text,
                                                       funkos_id int,
                                                       nombre text,
                                                       stock int,
                                                       precio decimal,
                                                       proveedor_nombre text,
                                                       PRIMARY KEY(proveedor_id, funkos_id)
                                                       )
                                                       """));
        await session.ExecuteAsync(new SimpleStatement("""
                                                       CREATE TABLE IF NOT EXISTS funkos_por_proveedor_categoria(
                                                           proveedor_id text,
                                                           categoria_id text,
                                                           funkos_id int,
                                                           nombre text,
                                                           proveedor_nombre text,
                                                           categoria_nombre text,
                                                           PRIMARY KEY((proveedor_id, categoria_id), funkos_id)
                                                       )
                                                       """));
    }
}
        
