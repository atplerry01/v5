namespace Whycespace.Engines.T2E.Business.Entitlement.Quota;

public record QuotaCommand(
    string Action,
    string EntityId,
    object Payload
);
