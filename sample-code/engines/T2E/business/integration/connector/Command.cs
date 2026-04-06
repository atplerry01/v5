namespace Whycespace.Engines.T2E.Business.Integration.Connector;

public record ConnectorCommand(
    string Action,
    string EntityId,
    object Payload
);
