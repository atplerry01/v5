namespace Whycespace.Engines.T2E.Business.Integration.EventBridge;

public record EventBridgeCommand(
    string Action,
    string EntityId,
    object Payload
);
