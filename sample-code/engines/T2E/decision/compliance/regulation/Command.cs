namespace Whycespace.Engines.T2E.Decision.Compliance.Regulation;

public record RegulationCommand(
    string Action,
    string EntityId,
    object Payload
);
