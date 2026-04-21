using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public static class OrderErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("OrderId is required and must not be empty.");

    public static DomainException MissingSourceReference()
        => new DomainInvariantViolationException("OrderSourceReference is required and must not be empty.");

    public static DomainException MissingDescription()
        => new DomainInvariantViolationException("Order description content reference is required and must not be empty.");

    public static DomainException InvalidStateTransition(OrderStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyConfirmed(OrderId id)
        => new DomainInvariantViolationException($"Order '{id.Value}' has already been confirmed and is immutable.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Order has already been initialized.");
}
