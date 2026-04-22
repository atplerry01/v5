namespace Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationResolution;

public sealed record ConfigurationResolvedEventSchema(
    Guid AggregateId,
    string DefinitionId,
    string ScopeId,
    string StateId,
    string ResolvedValue,
    DateTimeOffset ResolvedAt);
