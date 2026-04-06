namespace Whycespace.Engines.T2E.Decision.Risk.Review;

public record ReviewCommand(
    string Action,
    string EntityId,
    object Payload
);
