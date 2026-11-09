using FluentAssertions;
using FunkoApiV._2.Config;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Config;

[TestFixture]
[Category("Unit")]
public class FunkoConfigTests {
    [Test]
    public void Init_ConfiguracionCompleta_ExponeConexiones() {
        FunkoConfig.Init(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["ConnectionStrings:Postgre"] = "postgres",
                ["ConnectionStrings:Mongo"] = "mongodb",
                ["ConnectionStrings:Redis"] = "redis",
                ["DatabaseSettings:Name"] = "funkos"
            })
            .Build());

        FunkoConfig.PostgresConnection.Should().Be("postgres");
        FunkoConfig.MongoConnection.Should().Be("mongodb");
        FunkoConfig.RedisConnection.Should().Be("redis");
        FunkoConfig.MongoDatabase.Should().Be("funkos");
    }

    [TestCase("Postgre", "Falta ConnectionStrings:Postgre")]
    [TestCase("Mongo", "Falta ConnectionStrings:Mongo")]
    [TestCase("Redis", "Falta ConnectionStrings:Redis")]
    public void ConnectionString_Ausente_LanzaErrorConNombreDeClave(string key, string message) {
        FunkoConfig.Init(new ConfigurationBuilder().AddInMemoryCollection().Build());

        var action = () => key switch {
            "Postgre" => FunkoConfig.PostgresConnection,
            "Mongo" => FunkoConfig.MongoConnection,
            _ => FunkoConfig.RedisConnection
        };

        action.Should().Throw<InvalidOperationException>().WithMessage(message);
    }

    [Test]
    public void MongoDatabase_Ausente_LanzaErrorConNombreDeClave() {
        FunkoConfig.Init(new ConfigurationBuilder().AddInMemoryCollection().Build());

        var action = () => FunkoConfig.MongoDatabase;

        action.Should().Throw<InvalidOperationException>().WithMessage("Falta DatabaseSettings:Name");
    }
}
