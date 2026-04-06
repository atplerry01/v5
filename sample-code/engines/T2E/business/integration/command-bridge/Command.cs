namespace Whycespace.Engines.T2E.Business.Integration.CommandBridge;

public record CommandBridgeCommand(
    string Action,
    string EntityId,
    object Payload
);
