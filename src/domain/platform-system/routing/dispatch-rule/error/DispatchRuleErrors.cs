using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.DispatchRule;

public static class DispatchRuleErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("DispatchRule has already been initialized.");

    public static DomainException RuleNameMissing() =>
        new DomainInvariantViolationException("DispatchRule requires a non-empty RuleName.");

    public static DomainException RouteRefMissing() =>
        new DomainInvariantViolationException("DispatchRule requires a non-empty RouteRef.");

    public static DomainException PriorityNegative() =>
        new DomainInvariantViolationException("DispatchRule Priority must be non-negative.");

    public static DomainException AlreadyInactive() =>
        new DomainInvariantViolationException("DispatchRule is already inactive.");
}
