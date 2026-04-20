namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public static class SurchargeErrors
{
    public static SurchargeDomainException MissingId()
        => new("SurchargeId is required and must not be empty.");

    public static SurchargeDomainException InvalidStateTransition(SurchargeStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static SurchargeDomainException PercentageOutOfRange(decimal value)
        => new($"SurchargeAmount '{value}' is out of the [0, 100] range required for Percentage basis.");
}

public sealed class SurchargeDomainException : Exception
{
    public SurchargeDomainException(string message) : base(message) { }
}
