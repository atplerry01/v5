namespace Whycespace.Engines.T2E.Business.Entitlement.Restriction;

public record RestrictionCommand(
    string Action,
    string EntityId,
    object Payload
);
