namespace Whycespace.Domain.BusinessSystem.Marketplace.Match;

public static class MatchErrors
{
    public static MatchDomainException MissingId()
        => new("MatchId is required and must not be empty.");

    public static MatchDomainException MissingSideA()
        => new("MatchSideReference for Side A is required and must not be empty.");

    public static MatchDomainException MissingSideB()
        => new("MatchSideReference for Side B is required and must not be empty.");

    public static MatchDomainException SidesCannotBeEqual(MatchSideReference side)
        => new($"Match sides must reference different parties. Both reference '{side.Value}'.");
}

public sealed class MatchDomainException : Exception
{
    public MatchDomainException(string message) : base(message) { }
}
