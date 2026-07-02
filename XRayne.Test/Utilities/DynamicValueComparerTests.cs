using System.Text.Json;
using System.Text.Json.Nodes;
using XRayne.Contracts.Utilities;

namespace XRayne.Test.Utilities;

public sealed class DynamicValueComparerTests
{
    [Fact]
    public void AreEqual_WhenStringsDifferOnlyByCase_ReturnsFalse()
    {
        DynamicValueComparer.AreEqual("abc", "ABC").Should().BeFalse();
    }

    [Fact]
    public void AreEqual_WhenNumericTypesDifferButValueSame_ReturnsTrue()
    {
        DynamicValueComparer.AreEqual(42, 42L).Should().BeTrue();
    }

    [Fact]
    public void AreEqual_WhenDictionariesHaveSameKeysInDifferentOrder_ReturnsTrue()
    {
        var left = new Dictionary<string, object?>
        {
            ["port"] = 5097,
            ["sessionLifetimeMinutes"] = 7200
        };
        var right = new Dictionary<string, object?>
        {
            ["sessionLifetimeMinutes"] = 7200,
            ["port"] = 5097L
        };

        DynamicValueComparer.AreEqual(left, right).Should().BeTrue();
    }

    [Fact]
    public void AreEqual_WhenCollectionsHaveDifferentOrder_ReturnsFalse()
    {
        DynamicValueComparer.AreEqual(new[] { 1, 2 }, new[] { 2, 1 }).Should().BeFalse();
    }

    [Fact]
    public void AreEqual_WhenJsonObjectsHaveSameShape_ReturnsTrue()
    {
        using var left = JsonDocument.Parse("""{"port":5097,"nested":{"enabled":true}}""");
        var right = JsonNode.Parse("""{"nested":{"enabled":true},"port":5097}""")!;

        DynamicValueComparer.AreEqual(left.RootElement, right).Should().BeTrue();
    }

    [Fact]
    public void AreEqual_WhenObjectsHaveSamePublicProperties_ReturnsTrue()
    {
        var left = new SampleOptions("localhost", 5097);
        var right = new SampleOptions("localhost", 5097);

        DynamicValueComparer.AreEqual(left, right).Should().BeTrue();
    }

    [Fact]
    public void AreEqual_WhenNestedObjectPropertyDiffers_ReturnsFalse()
    {
        var left = new
        {
            Panel = new SampleOptions("localhost", 5097)
        };
        var right = new
        {
            Panel = new SampleOptions("localhost", 5098)
        };

        DynamicValueComparer.AreEqual(left, right).Should().BeFalse();
    }

    private sealed record SampleOptions(string Host, int Port);
}
