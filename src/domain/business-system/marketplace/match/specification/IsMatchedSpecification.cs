namespace Whycespace.Domain.BusinessSystem.Marketplace.Match;

public sealed class IsMatchedSpecification
{
    public bool IsSatisfiedBy(MatchStatus status)
    {
        return status == MatchStatus.Matched;
    }
}
