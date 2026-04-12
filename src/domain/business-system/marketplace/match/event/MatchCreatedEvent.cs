namespace Whycespace.Domain.BusinessSystem.Marketplace.Match;

public sealed record MatchCreatedEvent(
    MatchId MatchId,
    MatchSideReference SideA,
    MatchSideReference SideB);
