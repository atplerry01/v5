namespace Whycespace.Engines.T2E.Decision.Compliance.Jurisdiction;

public record JurisdictionCommand(
    string Action,
    string EntityId,
    object Payload
);
