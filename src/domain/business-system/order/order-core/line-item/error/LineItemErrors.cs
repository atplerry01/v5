namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public static class LineItemErrors
{
    public static LineItemDomainException MissingId()
        => new("LineItemId is required and must not be empty.");

    public static LineItemDomainException MissingOrderRef()
        => new("LineItem must reference an order.");

    public static LineItemDomainException InvalidStateTransition(LineItemStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static LineItemDomainException CancelledImmutable(LineItemId id)
        => new($"LineItem '{id.Value}' is cancelled and cannot be mutated.");
}

public sealed class LineItemDomainException : Exception
{
    public LineItemDomainException(string message) : base(message) { }
}
