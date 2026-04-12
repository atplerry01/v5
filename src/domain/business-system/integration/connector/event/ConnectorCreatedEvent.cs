namespace Whycespace.Domain.BusinessSystem.Integration.Connector;

public sealed record ConnectorCreatedEvent(ConnectorId ConnectorId, ConnectorTargetId TargetId);
