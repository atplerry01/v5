namespace Whycespace.Domain.BusinessSystem.Inventory.Transfer;

public static class TransferErrors
{
    public static TransferDomainException MissingId()
        => new("TransferId is required and must not be empty.");

    public static TransferDomainException SameSourceAndDestination()
        => new("Source and destination warehouse must be different.");

    public static TransferDomainException InvalidQuantity()
        => new("Transfer quantity must be positive.");

    public static TransferDomainException AlreadyCompleted(TransferId id)
        => new($"Transfer '{id.Value}' has already been completed.");

    public static TransferDomainException AlreadyCancelled(TransferId id)
        => new($"Transfer '{id.Value}' has already been cancelled.");

    public static TransferDomainException InvalidStateTransition(TransferStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class TransferDomainException : Exception
{
    public TransferDomainException(string message) : base(message) { }
}
