namespace Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationAssignment;

public sealed record ConfigurationAssignedEventSchema(
    Guid AggregateId,
    string DefinitionId,
    string ScopeId,
    string Value);

public sealed record ConfigurationAssignmentRevokedEventSchema(
    Guid AggregateId);
