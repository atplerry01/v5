using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Violation;

public static class ViolationErrors
{
    public static DomainException MissingRuleReference() =>
        new("Violation must reference a rule.");

    public static DomainException MissingSourceReference() =>
        new("Violation must reference a source.");

    public static DomainException MissingReason() =>
        new("Violation must include a reason.");

    public static DomainException ViolationAlreadyResolved() =>
        new("Violation has already been resolved.");

    public static DomainException ViolationNotOpen() =>
        new("Violation must be in Open status to acknowledge.");

    public static DomainException ViolationNotAcknowledged() =>
        new("Violation must be in Acknowledged status to resolve.");

    public static DomainException MissingResolution() =>
        new("Resolution description must be non-empty.");

    public static DomainInvariantViolationException EmptyViolationId() =>
        new("Invariant violated: ViolationId cannot be empty.");

    public static DomainInvariantViolationException OrphanViolation() =>
        new("Invariant violated: violation must reference both a rule and a source.");

    public static DomainException InvalidSeverityActionCombination(ViolationSeverity severity, EnforcementAction action) =>
        new($"Invalid enforcement combination: severity={severity} cannot recommend action={action}.");
}
