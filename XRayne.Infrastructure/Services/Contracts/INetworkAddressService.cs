using System.Net;

namespace XRayne.Infrastructure.Services;

public interface INetworkAddressService
{
    string GetLocalServerIpAddress();

    Task<string> GetPublicIpAddressAsync(CancellationToken cancellationToken = default);

    Task<string> ResolveServerIpAddressAsync(CancellationToken cancellationToken = default);

    bool IsUsableIPv4Address(IPAddress address);

    bool IsPublicIPv4Address(IPAddress address);

    string NormalizeUsableIPv4Address(string value);

    string NormalizePublicIPv4Address(string value);
}
