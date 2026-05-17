using System.Reflection;
using XRayne.Contracts.Configurations;

namespace XRayne.Infrastructure.Services.PanelSettings;

public sealed record SettingsDiffResult(IReadOnlyList<string> ChangedFields, RestartImpact MaxImpact);

public static class SettingsDiff
{
    private static readonly (PropertyInfo Property, RestartImpact Impact)[] Properties = typeof(PanelOptions)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanRead && p.CanWrite)
        .Select(p => (p, p.GetCustomAttribute<RestartImpactAttribute>()?.Impact ?? RestartImpact.None))
        .ToArray();

    // Сравнение строк — ordinal/case-sensitive (через object.Equals).
    // Все текущие поля технические (пути, IP, CIDR); для culture-sensitive
    // полей понадобится отдельная стратегия сравнения.
    // Имена в результате camelCase — должны совпадать с JSON-контрактом.
    public static SettingsDiffResult Compute(PanelOptions left, PanelOptions right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var changed = new List<string>();
        var maxImpact = RestartImpact.None;

        foreach (var (property, impact) in Properties)
        {
            var leftValue = property.GetValue(left);
            var rightValue = property.GetValue(right);

            if (Equals(leftValue, rightValue))
            {
                continue;
            }

            changed.Add(Camelize(property.Name));
            if (impact > maxImpact)
            {
                maxImpact = impact;
            }
        }

        return new SettingsDiffResult(changed, maxImpact);
    }

    private static string Camelize(string name) =>
        name.Length == 0 ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
