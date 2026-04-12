namespace Whycespace.Domain.BusinessSystem.Marketplace.Match;

public sealed class HasValidReferencesSpecification
{
    public bool IsSatisfiedBy(MatchSideReference sideA, MatchSideReference sideB)
    {
        return sideA != default && sideB != default && sideA != sideB;
    }
}
