namespace Whycespace.Engines.T2E.Decision.Risk.Rating;

public record RatingCommand(
    string Action,
    string EntityId,
    object Payload
);
