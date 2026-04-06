namespace Whycespace.Engines.T2E.Decision.Risk.IncidentRisk;

public record IncidentRiskCommand(
    string Action,
    string EntityId,
    object Payload
);
