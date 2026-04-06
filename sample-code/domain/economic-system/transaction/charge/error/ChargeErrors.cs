namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public static class ChargeErrors
{
    public static DomainException InvalidChargeAmount(decimal amount)
        => new("CHARGE.INVALID_AMOUNT", $"Charge amount must be greater than zero. Got: {amount}");

    public static DomainException ChargeAlreadyApplied(Guid chargeId)
        => new("CHARGE.ALREADY_APPLIED", $"Charge '{chargeId}' has already been applied.");

    public static DomainException ChargeNotReversible(Guid chargeId, string currentStatus)
        => new("CHARGE.NOT_REVERSIBLE", $"Charge '{chargeId}' cannot be reversed from status '{currentStatus}'.");
}
