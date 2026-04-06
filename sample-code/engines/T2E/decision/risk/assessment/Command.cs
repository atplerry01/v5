namespace Whycespace.Engines.T2E.Decision.Risk.Assessment;

public record AssessmentCommand(
    string Action,
    string EntityId,
    object Payload
);
