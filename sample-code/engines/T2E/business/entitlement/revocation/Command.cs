namespace Whycespace.Engines.T2E.Business.Entitlement.Revocation;

public record RevocationCommand(
    string Action,
    string EntityId,
    object Payload
);
