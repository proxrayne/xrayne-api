using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Contracts.Utilities;

/// <summary>
/// Resolves public API enum wire names.
/// </summary>
public static class EnumWireNames
{
    private static readonly ConcurrentDictionary<Type, EnumWireNameMap> Maps = new();

    /// <summary>
    /// Gets the configured wire name for an enum value.
    /// </summary>
    public static string GetName<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        return GetName(typeof(TEnum), value);
    }

    /// <summary>
    /// Gets the configured wire name for an enum value.
    /// </summary>
    public static string GetName(Type enumType, object value)
    {
        var map = GetMap(enumType);

        if (map.ValueToName.TryGetValue(value, out var name))
        {
            return name;
        }

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Parses a public API enum wire name.
    /// </summary>
    public static bool TryParse(Type enumType, string value, out object? result)
    {
        var map = GetMap(enumType);

        if (map.NameToValue.TryGetValue(value, out result))
        {
            return true;
        }

        if (TryParseNumeric(enumType, value, out result))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets all accepted wire names for an enum type.
    /// </summary>
    public static IReadOnlyCollection<string> GetNames(Type enumType)
    {
        return [.. GetMap(enumType).NameToValue.Keys];
    }

    private static EnumWireNameMap GetMap(Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("Type must be an enum.", nameof(enumType));
        }

        return Maps.GetOrAdd(enumType, CreateMap);
    }

    private static EnumWireNameMap CreateMap(Type enumType)
    {
        var nameToValue = new Dictionary<string, object>(StringComparer.Ordinal);
        var valueToName = new Dictionary<object, string>();

        foreach (var name in Enum.GetNames(enumType))
        {
            var member = enumType.GetMember(name, BindingFlags.Public | BindingFlags.Static).Single();
            var attribute = member.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();
            var wireName = attribute?.Name ?? name;
            var value = Enum.Parse(enumType, name);

            nameToValue.Add(wireName, value);
            valueToName.Add(value, wireName);
        }

        return new EnumWireNameMap(nameToValue, valueToName);
    }

    private static bool TryParseNumeric(Type enumType, string value, out object? result)
    {
        result = null;
        var underlyingType = Enum.GetUnderlyingType(enumType);

        try
        {
            var number = Convert.ChangeType(value, underlyingType);
            if (number is null || !Enum.IsDefined(enumType, number))
            {
                return false;
            }

            result = Enum.ToObject(enumType, number);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
        catch (InvalidCastException)
        {
            return false;
        }
    }

    private sealed record EnumWireNameMap(
        IReadOnlyDictionary<string, object> NameToValue,
        IReadOnlyDictionary<object, string> ValueToName);
}
