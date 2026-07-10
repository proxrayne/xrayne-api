using System.Security.Cryptography;

namespace Infrastructure.Utilities;

/// <summary>
/// Generates random readable names for panel resources.
/// </summary>
public static class RandomNameGenerator
{
    private const int MinNumber = 1000;
    private const int MaxNumberExclusive = 10_000;

    private static readonly string[] Adjectives =
    [
        "silent",
        "rapid",
        "hidden",
        "bright",
        "frozen",
        "wild",
        "atomic",
        "cosmic",
        "silver",
        "golden",
        "crimson",
        "azure",
        "lunar",
        "solar",
        "stellar",
        "neon",
        "shadow",
        "swift",
        "brave",
        "calm",
        "clever",
        "noble",
        "mystic",
        "ember",
        "glacial",
        "electric",
        "velvet",
        "iron",
        "onyx",
        "opal",
        "echo",
        "vivid",
        "lucid",
        "prime",
        "arcane",
        "wander"
    ];

    private static readonly string[] Nouns =
    [
        "fox",
        "wolf",
        "falcon",
        "tiger",
        "panda",
        "raven",
        "dragon",
        "storm",
        "lynx",
        "hawk",
        "eagle",
        "bear",
        "otter",
        "orca",
        "phoenix",
        "comet",
        "nebula",
        "nova",
        "orbit",
        "river",
        "mountain",
        "forest",
        "ember",
        "flame",
        "frost",
        "wave",
        "stone",
        "blade",
        "spark",
        "signal",
        "pulse",
        "cipher",
        "vector",
        "anchor",
        "voyager",
        "summit"
    ];

    /// <summary>
    /// Generates a random name in adjective-noun-number format.
    /// </summary>
    public static string Generate()
    {
        var adjective = Adjectives[RandomNumberGenerator.GetInt32(Adjectives.Length)];
        var noun = Nouns[RandomNumberGenerator.GetInt32(Nouns.Length)];
        var number = RandomNumberGenerator.GetInt32(MinNumber, MaxNumberExclusive);

        return $"{adjective}-{noun}-{number}";
    }
}
