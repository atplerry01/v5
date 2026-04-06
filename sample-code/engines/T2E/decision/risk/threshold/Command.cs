namespace Whycespace.Engines.T2E.Decision.Risk.Threshold;

public record ThresholdCommand(
    string Action,
    string EntityId,
    object Payload
);
