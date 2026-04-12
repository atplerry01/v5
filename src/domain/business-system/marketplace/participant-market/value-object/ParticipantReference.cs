namespace Whycespace.Domain.BusinessSystem.Marketplace.ParticipantMarket;

public readonly record struct ParticipantReference
{
    public Guid ParticipantId { get; }
    public Guid MarketId { get; }

    public ParticipantReference(Guid participantId, Guid marketId)
    {
        if (participantId == Guid.Empty)
            throw new ArgumentException("ParticipantId must not be empty.", nameof(participantId));

        if (marketId == Guid.Empty)
            throw new ArgumentException("MarketId must not be empty.", nameof(marketId));

        ParticipantId = participantId;
        MarketId = marketId;
    }
}
