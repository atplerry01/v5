using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Rule;

public static class EnforcementRuleErrors
{
    public static DomainException DuplicateRuleCode() =>
        new("Rule code already exists.");

    public static DomainException InvalidScope() =>
        new("Rule must have a valid scope.");

    public static DomainException InvalidSeverity() =>
        new("Rule must have a valid severity.");

    public static DomainException InvalidStateTransition() =>
        new("Invalid rule status transition.");
}
