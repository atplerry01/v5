namespace Whycespace.Engines.T2E.Business.Entitlement.Limit;

public record LimitCommand(
    string Action,
    string EntityId,
    object Payload
);
