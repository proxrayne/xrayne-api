using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Services;

namespace XRayne.Test.Architecture;

public sealed class PathProviderUsageTests
{
    // Whitelist для прямого чтения overridable путей; остальные ходят через IProjectPathResolver.
    private static readonly HashSet<string> AllowedDirectAccessors =
    [
        typeof(ProjectPathResolver).FullName!,
        typeof(ProjectPaths).FullName!,
        typeof(PathProvider).FullName!
    ];

    private static readonly string[] GuardedProperties =
    [
        nameof(ProjectPaths.CertificatesDirectory),
        nameof(ProjectPaths.GeoResourcesDirectory)
    ];

    [Fact]
    public void NoCodeOutsideResolverOrBootstrap_ReadsOverridablePaths_Directly()
    {
        var infraPath = typeof(ProjectPathResolver).Assembly.Location;
        using var module = ModuleDefinition.ReadModule(infraPath);

        var violations = new List<string>();
        foreach (var type in module.GetTypes())
        {
            if (AllowedDirectAccessors.Contains(type.FullName))
            {
                continue;
            }

            foreach (var method in type.Methods)
            {
                if (!method.HasBody)
                {
                    continue;
                }

                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode != OpCodes.Call && instruction.OpCode != OpCodes.Callvirt)
                    {
                        continue;
                    }

                    if (instruction.Operand is not MethodReference target)
                    {
                        continue;
                    }

                    if (target.DeclaringType.FullName != typeof(ProjectPaths).FullName)
                    {
                        continue;
                    }

                    var propertyName = target.Name.StartsWith("get_") ? target.Name[4..] : target.Name;
                    if (GuardedProperties.Contains(propertyName))
                    {
                        violations.Add($"{type.FullName}.{method.Name} reads ProjectPaths.{propertyName}");
                    }
                }
            }
        }

        violations.Should().BeEmpty(
            "overridable paths must be read via IProjectPathResolver so panel settings take effect; offenders: {0}",
            string.Join("; ", violations));
    }
}
