namespace Whycespace.Domain.BusinessSystem.Inventory.Lot;

public static class LotErrors
{
    public static LotDomainException MissingId()
        => new("LotId is required and must not be empty.");

    public static LotDomainException MissingOrigin()
        => new("Lot origin is required and must not be empty.");

    public static LotDomainException AlreadySealed(LotId id)
        => new($"Lot '{id.Value}' has already been sealed and is immutable.");

    public static LotDomainException InvalidStateTransition(LotStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class LotDomainException : Exception
{
    public LotDomainException(string message) : base(message) { }
}
