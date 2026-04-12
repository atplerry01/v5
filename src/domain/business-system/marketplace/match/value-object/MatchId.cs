namespace Whycespace.Domain.BusinessSystem.Marketplace.Match;

public readonly record struct MatchId
{
    public Guid Value { get; }

    public MatchId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MatchId value must not be empty.", nameof(value));
        Value = value;
    }
}
