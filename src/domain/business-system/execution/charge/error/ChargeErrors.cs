namespace Whycespace.Domain.BusinessSystem.Execution.Charge;

public static class ChargeErrors
{
    public static ChargeDomainException MissingId()
        => new("ChargeId is required and must not be empty.");

    public static ChargeDomainException MissingCostSourceId()
        => new("CostSourceId is required and must not be empty.");

    public static ChargeDomainException ItemRequired()
        => new("Charge must contain at least one charge item.");

    public static ChargeDomainException InvalidStateTransition(ChargeStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ChargeDomainException AlreadyCharged(ChargeId id)
        => new($"Charge '{id.Value}' has already been charged.");

    public static ChargeDomainException AlreadyReversed(ChargeId id)
        => new($"Charge '{id.Value}' has already been reversed.");
}

public sealed class ChargeDomainException : Exception
{
    public ChargeDomainException(string message) : base(message) { }
}
