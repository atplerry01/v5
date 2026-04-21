using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public static class LineItemErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("LineItemId is required and must not be empty.");

    public static DomainException MissingOrderRef()
        => new DomainInvariantViolationException("LineItem must reference an order.");

    public static DomainException MissingSubject()
        => new DomainInvariantViolationException("LineItem must declare a subject reference.");

    public static DomainException MissingQuantity()
        => new DomainInvariantViolationException("LineItem must declare a positive quantity.");

    public static DomainException InvalidStateTransition(LineItemStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException CancelledImmutable(LineItemId id)
        => new DomainInvariantViolationException($"LineItem '{id.Value}' is cancelled and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("LineItem has already been initialized.");
}
