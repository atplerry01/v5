namespace Whycespace.Engines.T2E.Business.Marketplace.Match;

public record MatchCommand(
    string Action,
    string EntityId,
    object Payload
);
