namespace Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyDefinition;

public sealed record PolicyDefinedEventSchema(
    Guid AggregateId,
    string Name,
    string ScopeClassification,
    string? ScopeContext,
    string ScopeActionMask,
    int Version);

public sealed record PolicyDeprecatedEventSchema(
    Guid AggregateId);
