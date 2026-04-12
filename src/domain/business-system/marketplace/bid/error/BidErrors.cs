namespace Whycespace.Domain.BusinessSystem.Marketplace.Bid;

public static class BidErrors
{
    public static BidDomainException MissingId()
        => new("BidId is required and must not be empty.");

    public static BidDomainException MissingReference()
        => new("Bid must reference a listing or market.");

    public static BidDomainException InvalidStateTransition(BidStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class BidDomainException : Exception
{
    public BidDomainException(string message) : base(message) { }
}
