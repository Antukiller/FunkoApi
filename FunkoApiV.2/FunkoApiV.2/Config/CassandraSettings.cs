namespace FunkoApiV._2.Config;

public class CassandraSettings {
    public const string SectionName = "Cassandra";
    public string[] Hosts { get; set; } = ["cassandra"];
    public int Port { get; set; } = 9042;
    public string Keyspace { get; set; } = "funkos";
}