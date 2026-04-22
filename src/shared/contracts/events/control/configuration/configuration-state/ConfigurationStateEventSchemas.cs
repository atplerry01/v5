namespace Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationState;

public sealed record ConfigurationStateSetEventSchema(
    Guid AggregateId,
    string DefinitionId,
    string Value,
    int Version);

public sealed record ConfigurationStateRevokedEventSchema(
    Guid AggregateId);
