using System.Reflection;

namespace Whycespace.Platform.Api.Core.Guards;

/// <summary>
/// E19.17.1 — Layer Isolation Guard.
/// Validates that the platform assembly does NOT reference forbidden assemblies.
///
/// FORBIDDEN:
/// - T0U engines (WhycePolicy, WhyceID, WhyceChain)
/// - T1M (WSS engines)
/// - T2E engines
/// - Domain aggregates/services
///
/// ALLOWED:
/// - Shared contracts
/// - Adapter interfaces
/// - Projections (read-only views)
///
/// Run at startup to fail-fast on layer violations.
/// </summary>
public static class LayerIsolationGuard
{
    private static readonly string[] ForbiddenNamespacePrefixes =
    {
        "Whycespace.Engines.T0U",
        "Whycespace.Engines.T1M",
        "Whycespace.Engines.T2E",
        "Whycespace.Engines.T3I",
        "Whycespace.Engines.T4A",
        "Whycespace.Domain"
    };

    private static readonly string[] ForbiddenAssemblyPrefixes =
    {
        "Whycespace.Engines",
        "Whycespace.Domain"
    };

    /// <summary>
    /// Scans all referenced assemblies from the platform API library.
    /// Throws if any forbidden assembly or namespace is detected.
    /// Call at startup (e.g., host bootstrap).
    ///
    /// NOTE: Validates the Platform.Api library assembly, not the FoundationHost
    /// composition root. The composition root is the DI wire-up point and is
    /// architecturally permitted to reference all assemblies for registration.
    /// </summary>
    public static LayerIsolationResult ValidateAssemblies()
    {
        var platformAssembly = typeof(LayerIsolationGuard).Assembly;
        var violations = new List<string>();

        // Check direct assembly references
        var referencedAssemblies = platformAssembly.GetReferencedAssemblies();
        foreach (var reference in referencedAssemblies)
        {
            foreach (var forbidden in ForbiddenAssemblyPrefixes)
            {
                if (reference.Name is not null &&
                    reference.Name.StartsWith(forbidden, StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add($"ASSEMBLY_VIOLATION: Platform references forbidden assembly '{reference.Name}'");
                }
            }
        }

        return new LayerIsolationResult(violations);
    }

    /// <summary>
    /// Scans platform types for namespace usage violations.
    /// Detects if any platform type inherits from or uses forbidden namespace types.
    /// </summary>
    public static LayerIsolationResult ValidateNamespaces()
    {
        var platformAssembly = typeof(LayerIsolationGuard).Assembly;
        var violations = new List<string>();

        foreach (var type in platformAssembly.GetTypes())
        {
            // Check base types
            var baseType = type.BaseType;
            if (baseType is not null && IsForbiddenNamespace(baseType.Namespace))
            {
                violations.Add(
                    $"NAMESPACE_VIOLATION: '{type.FullName}' inherits from forbidden type '{baseType.FullName}'");
            }

            // Check implemented interfaces
            foreach (var iface in type.GetInterfaces())
            {
                if (IsForbiddenNamespace(iface.Namespace))
                {
                    violations.Add(
                        $"NAMESPACE_VIOLATION: '{type.FullName}' implements forbidden interface '{iface.FullName}'");
                }
            }

            // Check fields for forbidden type references
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Static |
                                                  BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (IsForbiddenNamespace(field.FieldType.Namespace))
                {
                    violations.Add(
                        $"FIELD_VIOLATION: '{type.FullName}.{field.Name}' references forbidden type '{field.FieldType.FullName}'");
                }
            }
        }

        return new LayerIsolationResult(violations);
    }

    /// <summary>
    /// Full validation: assemblies + namespaces. Throws on violation.
    /// </summary>
    public static void EnforceAtStartup()
    {
        var assemblyResult = ValidateAssemblies();
        var namespaceResult = ValidateNamespaces();

        var allViolations = new List<string>();
        allViolations.AddRange(assemblyResult.Violations);
        allViolations.AddRange(namespaceResult.Violations);

        if (allViolations.Count > 0)
        {
            throw new PlatformLayerViolationException(allViolations);
        }
    }

    private static bool IsForbiddenNamespace(string? ns)
    {
        if (string.IsNullOrWhiteSpace(ns))
            return false;

        foreach (var forbidden in ForbiddenNamespacePrefixes)
        {
            if (ns.StartsWith(forbidden, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}

public sealed record LayerIsolationResult(IReadOnlyList<string> Violations)
{
    public bool IsCompliant => Violations.Count == 0;
}

public sealed class PlatformLayerViolationException : Exception
{
    public IReadOnlyList<string> Violations { get; }

    public PlatformLayerViolationException(IReadOnlyList<string> violations)
        : base($"Platform layer isolation violated ({violations.Count} violations):\n{string.Join("\n", violations)}")
    {
        Violations = violations;
    }
}
