namespace Whycespace.Domain.BusinessSystem.Billing.Receivable;

public static class ReceivableErrors
{
    public static ReceivableDomainException MissingId()
        => new("ReceivableId is required and must not be empty.");

    public static ReceivableDomainException AlreadySettled(ReceivableId id)
        => new($"Receivable '{id.Value}' has already been settled.");

    public static ReceivableDomainException AlreadyWrittenOff(ReceivableId id)
        => new($"Receivable '{id.Value}' has already been written off.");

    public static ReceivableDomainException InvalidStateTransition(ReceivableStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class ReceivableDomainException : Exception
{
    public ReceivableDomainException(string message) : base(message) { }
}
