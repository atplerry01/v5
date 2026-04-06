namespace Whycespace.Engines.T2E.Decision.Compliance.Attestation;

public record AttestationCommand(
    string Action,
    string EntityId,
    object Payload
);
