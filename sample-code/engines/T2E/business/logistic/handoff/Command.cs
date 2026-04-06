namespace Whycespace.Engines.T2E.Business.Logistic.Handoff;

public record HandoffCommand(
    string Action,
    string EntityId,
    object Payload
);
