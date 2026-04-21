using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public static class CancellationErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("CancellationId is required and must not be empty.");

    public static DomainException MissingOrderRef()
        => new DomainInvariantViolationException("Cancellation must reference an order.");

    public static DomainException InvalidStateTransition(CancellationStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyTerminal(CancellationId id, CancellationStatus status)
        => new DomainInvariantViolationException($"Cancellation '{id.Value}' is already terminal ({status}).");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Cancellation has already been initialized.");
}
