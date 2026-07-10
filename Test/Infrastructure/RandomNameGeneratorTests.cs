using Infrastructure.Utilities;

namespace Test.Infrastructure;

/// <summary>
/// Tests random readable name generation.
/// </summary>
public sealed class RandomNameGeneratorTests
{
    private static readonly HashSet<string> Adjectives =
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

    private static readonly HashSet<string> Nouns =
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

    [Fact]
    public void Generate_ReturnsExpectedNameShape()
    {
        var name = RandomNameGenerator.Generate();

        var parts = name.Split('-');

        parts.Should().HaveCount(3);
        Adjectives.Should().Contain(parts[0]);
        Nouns.Should().Contain(parts[1]);
        parts[2].Should().MatchRegex(@"^\d{4}$");
    }

    [Fact]
    public void Generate_ReturnsNumberInExpectedRange()
    {
        var name = RandomNameGenerator.Generate();

        var number = int.Parse(name.Split('-')[2]);

        number.Should().BeInRange(1000, 9999);
    }
}
