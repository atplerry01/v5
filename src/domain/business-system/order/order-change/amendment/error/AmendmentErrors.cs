using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public static class AmendmentErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("AmendmentId is required and must not be empty.");

    public static DomainException MissingOrderRef()
        => new DomainInvariantViolationException("Amendment must reference an order.");

    public static DomainException InvalidStateTransition(AmendmentStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyTerminal(AmendmentId id, AmendmentStatus status)
        => new DomainInvariantViolationException($"Amendment '{id.Value}' is already terminal ({status}).");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Amendment has already been initialized.");
}
