namespace Whycespace.Domain.BusinessSystem.Marketplace.SettlementMarket;

public static class SettlementMarketErrors
{
    public static SettlementMarketDomainException MissingId()
        => new("SettlementMarketId is required and must not be empty.");

    public static SettlementMarketDomainException MissingTerms()
        => new("Settlement market must have defined terms before sealing.");

    public static SettlementMarketDomainException InvalidStateTransition(SettlementMarketStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class SettlementMarketDomainException : Exception
{
    public SettlementMarketDomainException(string message) : base(message) { }
}
