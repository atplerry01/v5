namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public static class OrderErrors
{
    public static OrderDomainException MissingId()
        => new("OrderId is required and must not be empty.");

    public static OrderDomainException MissingSourceReference()
        => new("OrderSourceReference is required and must not be empty.");

    public static OrderDomainException InvalidStateTransition(OrderStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static OrderDomainException AlreadyConfirmed(OrderId id)
        => new($"Order '{id.Value}' has already been confirmed and is immutable.");
}

public sealed class OrderDomainException : Exception
{
    public OrderDomainException(string message) : base(message) { }
}
