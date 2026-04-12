namespace Whycespace.Domain.BusinessSystem.Marketplace.Match;

public readonly record struct MatchSideReference
{
    public Guid Value { get; }

    public MatchSideReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MatchSideReference value must not be empty.", nameof(value));
        Value = value;
    }
}
