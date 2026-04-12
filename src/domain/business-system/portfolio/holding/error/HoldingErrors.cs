namespace Whycespace.Domain.BusinessSystem.Portfolio.Holding;

public static class HoldingErrors
{
    public static HoldingDomainException MissingId()
        => new("HoldingId is required and must not be empty.");

    public static HoldingDomainException InvalidStateTransition(HoldingStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static HoldingDomainException AssetReferenceRequired()
        => new("Holding must reference an asset.");

    public static HoldingDomainException PortfolioReferenceRequired()
        => new("Holding must reference a portfolio.");

    public static HoldingDomainException QuantityMustBePositive()
        => new("Holding quantity must be greater than zero.");
}

public sealed class HoldingDomainException : Exception
{
    public HoldingDomainException(string message) : base(message) { }
}
