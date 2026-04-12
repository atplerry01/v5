namespace Whycespace.Domain.BusinessSystem.Inventory.Writeoff;

public static class WriteoffErrors
{
    public static WriteoffDomainException MissingId()
        => new("WriteoffId is required and must not be empty.");

    public static WriteoffDomainException MissingReference()
        => new("Writeoff must reference a stock or item.");

    public static WriteoffDomainException InvalidQuantity()
        => new("Writeoff quantity must be positive.");

    public static WriteoffDomainException MissingReason()
        => new("Writeoff reason is required — no silent write-offs.");

    public static WriteoffDomainException AlreadyConfirmed(WriteoffId id)
        => new($"Writeoff '{id.Value}' has already been confirmed and is irreversible.");

    public static WriteoffDomainException InvalidStateTransition(WriteoffStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class WriteoffDomainException : Exception
{
    public WriteoffDomainException(string message) : base(message) { }
}
