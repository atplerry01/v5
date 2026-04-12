namespace Whycespace.Domain.BusinessSystem.Marketplace.ParticipantMarket;

public static class ParticipantMarketErrors
{
    public static ParticipantMarketDomainException MissingId()
        => new("ParticipantMarketId is required and must not be empty.");

    public static ParticipantMarketDomainException MissingReference()
        => new("Participant-market must link a participant to a market.");

    public static ParticipantMarketDomainException InvalidStateTransition(ParticipantMarketStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class ParticipantMarketDomainException : Exception
{
    public ParticipantMarketDomainException(string message) : base(message) { }
}
