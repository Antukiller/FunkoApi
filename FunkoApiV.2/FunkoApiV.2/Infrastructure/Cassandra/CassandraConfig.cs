using Cassandra;
using FunkoApiV._2.Config;
using Serilog;
using ISession = Cassandra.ISession;

namespace FunkoApiV._2.Infrastructure.Cassandra;

public static class CassandraConfig {
    public static IServiceCollection AddCassandra(this IServiceCollection services, IConfiguration config) {
        var settings = config
            .GetSection(CassandraSettings.SectionName)
            .Get<CassandraSettings>() ?? new CassandraSettings();
        
        
        Log.Information("Conectando a Cassandra: {Hosts}:{Port}",
            string.Join(",", settings.Hosts), settings.Port);
        
        ISession session;

        for (var i = 1;  ; i++) {
            try {
                var cluster = Cluster.Builder()
                    .AddContactPoints(settings.Hosts)
                    .WithPort(settings.Port)
                    .Build();
                session = cluster.Connect();
                break;
            } catch (NoHostAvailableException) when(i < 50) {
                Log.Information("Cassandra aun no acepta conexiones (intento {Intento}/30)...", i);
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }

        services.AddSingleton(settings);
        services.AddSingleton<ISession>(session);
        return services;
    }
}