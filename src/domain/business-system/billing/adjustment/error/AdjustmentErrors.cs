namespace Whycespace.Domain.BusinessSystem.Billing.Adjustment;

public static class AdjustmentErrors
{
    public static AdjustmentDomainException MissingId()
        => new("AdjustmentId is required and must not be empty.");

    public static AdjustmentDomainException MissingReason()
        => new("Adjustment must have a reason.");

    public static AdjustmentDomainException AlreadyApplied(AdjustmentId id)
        => new($"Adjustment '{id.Value}' has already been applied.");

    public static AdjustmentDomainException AlreadyVoided(AdjustmentId id)
        => new($"Adjustment '{id.Value}' has already been voided.");

    public static AdjustmentDomainException InvalidStateTransition(AdjustmentStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class AdjustmentDomainException : Exception
{
    public AdjustmentDomainException(string message) : base(message) { }
}
