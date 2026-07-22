using Contracts.Utilities;

namespace Test.Utilities;

/// <summary>
/// Tests xray-core secret generation.
/// </summary>
public sealed class XraySecretGeneratorTests
{
    [Fact]
    public void GenerateShadowsocksPassword_ReturnsBase64UrlSecret()
    {
        var password = XraySecretGenerator.GeneratePassword();

        password.Should().HaveLength(43);
        password.Should().MatchRegex("^[A-Za-z0-9_-]+$");
    }

    [Fact]
    public void GenerateHysteriaPassword_ReturnsBase64UrlSecret()
    {
        var password = XraySecretGenerator.GenerateHysteriaPassword();

        password.Should().HaveLength(43);
        password.Should().MatchRegex("^[A-Za-z0-9_-]+$");
    }

    [Fact]
    public void GenerateSeq_ReturnsBase64UrlSecret()
    {
        var seq = XraySecretGenerator.GenerateSeq();

        seq.Should().HaveLength(22);
        seq.Should().MatchRegex("^[A-Za-z0-9_-]+$");
    }

    [Fact]
    public void GenerateBase64UrlSecret_WhenByteCountIsInvalid_Throws()
    {
        var act = () => XraySecretGenerator.GenerateBase64UrlSecret(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
