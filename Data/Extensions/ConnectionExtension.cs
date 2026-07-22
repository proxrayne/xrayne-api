using System.Globalization;
using Data.Entities;
using Xray.Config.Models;

namespace Data.Extensions;

/// <summary>
/// Provides Xray client conversion helpers for connection entities.
/// </summary>
public static class ConnectionExtension
{
    /// <summary>
    /// Creates a unique Xray client email from the provided parts and connection identifier.
    /// </summary>
    public static string GetUniqEmail(this ConnectionEntity connection, params string[] args)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return string.Join(".", [.. args, connection.Id.ToString(CultureInfo.InvariantCulture)]);
    }

    /// <summary>
    /// Converts the connection into a VLESS inbound client.
    /// </summary>
    public static VlessClient ToVlessClient(this ConnectionEntity connection, string email, int? level = null)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(email);

        return new VlessClient
        {
            Id = connection.Uuid.ToString(),
            Flow = connection.Flow,
            Email = email,
            Level = level
        };
    }

    /// <summary>
    /// Converts the connection into a VMess inbound client.
    /// </summary>
    public static VMessClient ToVMessClient(this ConnectionEntity connection, string email, int? level = null)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(email);

        return new VMessClient
        {
            Id = connection.Uuid.ToString(),
            Email = email,
            Level = level
        };
    }

    /// <summary>
    /// Converts the connection into a Trojan inbound client.
    /// </summary>
    public static TrojanClient ToTrojanClient(this ConnectionEntity connection, string email, int? level = null)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(email);

        return new TrojanClient
        {
            Password = connection.Password,
            Email = email,
            Level = level
        };
    }

    /// <summary>
    /// Converts the connection into a Shadowsocks inbound client.
    /// </summary>
    public static ShadowSocksClient ToShadowSocksClient(this ConnectionEntity connection, string email, int? level = null)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(email);

        return new ShadowSocksClient
        {
            Password = connection.Password,
            Method = connection.Method,
            Email = email,
            Level = level
        };
    }

    /// <summary>
    /// Converts the connection into a Hysteria inbound client.
    /// </summary>
    public static HysteriaInboundClient ToHysteriaClient(this ConnectionEntity connection, string email, int? level = null)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(email);

        return new HysteriaInboundClient
        {
            Auth = connection.Password,
            Email = email,
            Level = level
        };
    }
}
