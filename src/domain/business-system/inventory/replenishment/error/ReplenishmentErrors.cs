namespace Whycespace.Domain.BusinessSystem.Inventory.Replenishment;

public static class ReplenishmentErrors
{
    public static ReplenishmentDomainException MissingId()
        => new("ReplenishmentId is required and must not be empty.");

    public static ReplenishmentDomainException InvalidThreshold()
        => new("Replenishment threshold cannot be negative.");

    public static ReplenishmentDomainException InvalidRestockQuantity()
        => new("Restock quantity must be positive.");

    public static ReplenishmentDomainException AlreadyDeactivated(ReplenishmentId id)
        => new($"Replenishment policy '{id.Value}' has already been deactivated.");

    public static ReplenishmentDomainException InvalidStateTransition(ReplenishmentStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class ReplenishmentDomainException : Exception
{
    public ReplenishmentDomainException(string message) : base(message) { }
}
