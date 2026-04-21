using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;

public static class PlanErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("PlanId is required and must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Plan must include a valid descriptor.");

    public static DomainException InvalidStateTransition(PlanStatus current, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{current}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Plan has already been initialized.");
}
