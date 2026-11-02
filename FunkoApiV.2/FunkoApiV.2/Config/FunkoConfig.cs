namespace FunkoApiV._2.Config;

public static class FunkoConfig {
    public static IConfiguration Config { get; private set; } = null!;

    public static void Init(IConfiguration configuration) => Config = configuration;

    public static string PostgresConnection =>
        Config.GetConnectionString("Postgre")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Postgre");

    public static string MongoConnection =>
        Config.GetConnectionString("Mongo")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Mongo");

    public static string MongoDatabase =>
        Config["DatabaseSettings:Name"]
        ?? throw new InvalidOperationException("Falta DatabaseSettings:Name");

    public static string RedisConnection =>
        Config.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Redis");
    
}