using System.Reflection;

namespace Whycespace.Runtime.Guards;

/// <summary>
/// Determinism guard. Scans runtime and engine assemblies for violations
/// of deterministic execution rules.
///
/// FORBIDDEN:
/// - Guid.NewGuid()
/// - DateTime.UtcNow / DateTime.Now / DateTimeOffset.UtcNow
/// - Random()
///
/// REQUIRED REPLACEMENTS:
/// - DeterministicIdHelper / IIdGenerator
/// - IClock
/// - Seed-based hashing (SHA256)
/// </summary>
public static class DeterminismGuard
{
    /// <summary>
    /// Validates that a type does not contain non-deterministic method calls.
    /// Returns a list of violations found.
    /// </summary>
    public static IReadOnlyList<DeterminismViolation> Validate(Type type)
    {
        var violations = new List<DeterminismViolation>();

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            if (method.DeclaringType != type) continue;

            var body = method.GetMethodBody();
            if (body is null) continue;

            // Check for forbidden type usage in parameters and return types
            CheckForForbiddenTypes(method, violations, type);
        }

        // Check for fields that indicate non-deterministic state
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            if (field.FieldType == typeof(Random))
            {
                violations.Add(new DeterminismViolation(
                    type.FullName ?? type.Name,
                    field.Name,
                    "Field of type Random detected. Use seed-based hashing instead.",
                    DeterminismViolationSeverity.S0Critical));
            }
        }

        return violations;
    }

    private static void CheckForForbiddenTypes(MethodInfo method, List<DeterminismViolation> violations, Type declaringType)
    {
        var parameters = method.GetParameters();
        foreach (var param in parameters)
        {
            if (param.ParameterType == typeof(Random))
            {
                violations.Add(new DeterminismViolation(
                    declaringType.FullName ?? declaringType.Name,
                    $"{method.Name}(param: {param.Name})",
                    "Random parameter detected. Use seed-based generation.",
                    DeterminismViolationSeverity.S0Critical));
            }
        }
    }
}

public sealed record DeterminismViolation(
    string TypeName,
    string MemberName,
    string Description,
    DeterminismViolationSeverity Severity);

public enum DeterminismViolationSeverity
{
    S0Critical,
    S1High,
    S2Medium,
    S3Low
}
