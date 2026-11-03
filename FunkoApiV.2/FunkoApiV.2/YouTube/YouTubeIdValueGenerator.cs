using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace FunkoApiV._2.Models;

public class YouTubeIdValueGenerator : ValueGenerator<string> {
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

    public static string GenerateId() =>
        new(Enumerable.Range(0, 11).Select(_ => Chars[Random.Shared.Next(Chars.Length)]).ToArray());

    public override string Next(EntityEntry entry) => GenerateId();

    public override bool GeneratesTemporaryValues => false;
}
