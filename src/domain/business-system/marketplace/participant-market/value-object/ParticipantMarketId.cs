namespace Whycespace.Domain.BusinessSystem.Marketplace.ParticipantMarket;

public readonly record struct ParticipantMarketId
{
    public Guid Value { get; }

    public ParticipantMarketId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ParticipantMarketId value must not be empty.", nameof(value));

        Value = value;
    }
}
