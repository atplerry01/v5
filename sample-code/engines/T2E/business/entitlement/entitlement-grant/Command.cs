namespace Whycespace.Engines.T2E.Business.Entitlement.EntitlementGrant;

public record EntitlementGrantCommand(
    string Action,
    string EntityId,
    object Payload
);
