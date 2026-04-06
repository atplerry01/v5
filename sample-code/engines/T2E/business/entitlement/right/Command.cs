namespace Whycespace.Engines.T2E.Business.Entitlement.Right;

public record RightCommand(
    string Action,
    string EntityId,
    object Payload
);
