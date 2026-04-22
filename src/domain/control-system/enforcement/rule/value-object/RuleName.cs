using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Rule;

/// <summary>
/// D-CONTENT-STR-EMBED-01 — typed structural-descriptive VO replacing the raw
/// <c>string RuleName</c> previously embedded on <see cref="EnforcementRuleAggregate"/>.
/// Wraps a non-empty name string; non-versioned, non-content (no lifecycle).
/// </summary>
public readonly record struct RuleName
{
    public string Value { get; }

    public RuleName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "RuleName must not be empty.");
        Value = value;
    }
}
