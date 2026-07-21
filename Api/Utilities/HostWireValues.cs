using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Utilities;
using Xray.Config.Enums;

namespace Api.Utilities;

/// <summary>
/// Converts host API wire values to persisted enum values.
/// </summary>
public static class HostWireValues
{
    private const string H1 = "http/1.1";
    private const string H2 = "h2";
    private const string H3 = "h3";

    /// <summary>
    /// Converts ALPN flags to API values.
    /// </summary>
    public static IReadOnlyList<string>? ToAlpnNames(ALPN? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        var names = new List<string>();
        if (value.Value.HasFlag(ALPN.H1))
        {
            names.Add(H1);
        }

        if (value.Value.HasFlag(ALPN.H2))
        {
            names.Add(H2);
        }

        if (value.Value.HasFlag(ALPN.H3))
        {
            names.Add(H3);
        }

        return names;
    }

    /// <summary>
    /// Parses API ALPN values into flags.
    /// </summary>
    public static ALPN? ParseAlpn(IReadOnlyCollection<string>? values)
    {
        if (values is null || values.Count == 0)
        {
            return null;
        }

        var result = default(ALPN);
        foreach (var value in values.Select(value => value.Trim()).Distinct(StringComparer.Ordinal))
        {
            result |= value switch
            {
                H1 => ALPN.H1,
                H2 => ALPN.H2,
                H3 => ALPN.H3,
                _ => throw new BadRequestException($"ALPN value '{value}' is invalid.")
            };
        }

        return result == default ? null : result;
    }

    /// <summary>
    /// Converts host security to an API value.
    /// </summary>
    public static string ToSecurityName(HostSecurity value)
    {
        return EnumWireNames.GetName(value);
    }

    /// <summary>
    /// Parses an API host security value.
    /// </summary>
    public static HostSecurity ParseSecurity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BadRequestException("Host security is required.");
        }

        if (EnumWireNames.TryParse(typeof(HostSecurity), value.Trim(), out var parsed)
            && parsed is HostSecurity security)
        {
            return security;
        }

        throw new BadRequestException($"Host security '{value}' is invalid.");
    }

    /// <summary>
    /// Converts TLS fingerprint to an API value.
    /// </summary>
    public static string ToFingerprintName(Fingerprint value)
    {
        var name = value.ToString();

        return name switch
        {
            "None" => string.Empty,
            "iOS" => "ios",
            "e360" => "360",
            _ when !string.IsNullOrWhiteSpace(name) => char.ToLowerInvariant(name[0]) + name[1..],
            _ => string.Empty
        };
    }

    /// <summary>
    /// Parses an API TLS fingerprint value.
    /// </summary>
    public static Fingerprint ParseFingerprint(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        var enumName = normalized switch
        {
            "" => "None",
            "360" => "e360",
            "ios" => "iOS",
            _ => normalized
        };

        if (Enum.TryParse<Fingerprint>(enumName, ignoreCase: true, out var fingerprint))
        {
            return fingerprint;
        }

        throw new BadRequestException($"Fingerprint '{value}' is invalid.");
    }
}
