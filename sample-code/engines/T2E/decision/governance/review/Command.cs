namespace Whycespace.Engines.T2E.Decision.Governance.Review;

public record ReviewCommand(
    string Action,
    string EntityId,
    object Payload
);
