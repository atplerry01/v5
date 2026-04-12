namespace Whycespace.Domain.BusinessSystem.Inventory.Movement;

public static class MovementErrors
{
    public static MovementDomainException MissingId()
        => new("MovementId is required and must not be empty.");

    public static MovementDomainException MissingSourceId()
        => new("MovementSourceId is required and must not be empty.");

    public static MovementDomainException MissingTargetId()
        => new("MovementTargetId is required and must not be empty.");

    public static MovementDomainException InvalidQuantity()
        => new("Movement quantity must be greater than zero.");

    public static MovementDomainException InvalidStateTransition(MovementStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static MovementDomainException AlreadyConfirmed(MovementId id)
        => new($"Movement '{id.Value}' has already been confirmed.");

    public static MovementDomainException AlreadyCancelled(MovementId id)
        => new($"Movement '{id.Value}' has already been cancelled.");
}

public sealed class MovementDomainException : Exception
{
    public MovementDomainException(string message) : base(message) { }
}
