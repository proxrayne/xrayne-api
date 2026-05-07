using System.Net;
using System.Net.Sockets;
using XRayne.Infrastructure.Services;

namespace XRayne.Test.Infrastructure;

public sealed class NetworkAddressServiceTests
{
    private readonly NetworkAddressService _service = new();

    [Theory]
    [InlineData("8.8.8.8", true)]
    [InlineData("127.0.0.1", false)]
    [InlineData("::1", false)]
    public void IsUsableIPv4Address_ReturnsExpectedResult(
        string value,
        bool expected)
    {
        var address = IPAddress.Parse(value);

        Assert.Equal(expected, _service.IsUsableIPv4Address(address));
    }

    [Theory]
    [InlineData("8.8.8.8", true)]
    [InlineData("1.1.1.1", true)]
    [InlineData("10.0.0.1", false)]
    [InlineData("172.16.0.1", false)]
    [InlineData("192.168.1.1", false)]
    [InlineData("100.64.0.1", false)]
    [InlineData("169.254.1.1", false)]
    [InlineData("192.0.2.1", false)]
    [InlineData("198.51.100.1", false)]
    [InlineData("203.0.113.1", false)]
    [InlineData("224.0.0.1", false)]
    public void IsPublicIPv4Address_ReturnsExpectedResult(
        string value,
        bool expected)
    {
        var address = IPAddress.Parse(value);

        Assert.Equal(expected, _service.IsPublicIPv4Address(address));
    }

    [Fact]
    public void IsPublicIPv4Address_ReturnsFalseForIPv6Address()
    {
        var address = IPAddress.Parse("2001:4860:4860::8888");

        Assert.False(_service.IsPublicIPv4Address(address));
    }

    [Fact]
    public void NormalizeUsableIPv4Address_TrimsAndNormalizesValidIPv4()
    {
        var result = _service.NormalizeUsableIPv4Address(" 8.8.8.8 ");

        Assert.Equal("8.8.8.8", result);
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    [InlineData("not-an-ip")]
    public void NormalizeUsableIPv4Address_RejectsInvalidOrLoopbackAddress(string value)
    {
        Assert.Throws<InvalidOperationException>(() => _service.NormalizeUsableIPv4Address(value));
    }

    [Fact]
    public void NormalizePublicIPv4Address_TrimsAndNormalizesPublicIPv4()
    {
        var result = _service.NormalizePublicIPv4Address(" 1.1.1.1 ");

        Assert.Equal("1.1.1.1", result);
    }

    [Theory]
    [InlineData("10.0.0.1")]
    [InlineData("192.168.1.1")]
    [InlineData("203.0.113.1")]
    [InlineData("2001:4860:4860::8888")]
    [InlineData("not-an-ip")]
    public void NormalizePublicIPv4Address_RejectsNonPublicIPv4Address(string value)
    {
        Assert.Throws<InvalidOperationException>(() => _service.NormalizePublicIPv4Address(value));
    }

    [Fact]
    public void IsUsableIPv4Address_ReturnsFalseForIPv6Address()
    {
        var address = IPAddress.Parse("2001:4860:4860::8888");

        Assert.Equal(AddressFamily.InterNetworkV6, address.AddressFamily);
        Assert.False(_service.IsUsableIPv4Address(address));
    }
}
