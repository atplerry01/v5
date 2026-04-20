namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public static class CancellationErrors
{
    public static CancellationDomainException MissingId()
        => new("CancellationId is required and must not be empty.");

    public static CancellationDomainException MissingOrderRef()
        => new("Cancellation must reference an order.");

    public static CancellationDomainException InvalidStateTransition(CancellationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static CancellationDomainException AlreadyTerminal(CancellationId id, CancellationStatus status)
        => new($"Cancellation '{id.Value}' is already terminal ({status}).");
}

public sealed class CancellationDomainException : Exception
{
    public CancellationDomainException(string message) : base(message) { }
}
