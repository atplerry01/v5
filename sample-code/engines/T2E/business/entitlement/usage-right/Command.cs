namespace Whycespace.Engines.T2E.Business.Entitlement.UsageRight;

public record UsageRightCommand(
    string Action,
    string EntityId,
    object Payload
);
