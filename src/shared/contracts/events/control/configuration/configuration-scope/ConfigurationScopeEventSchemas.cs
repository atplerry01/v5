namespace Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationScope;

public sealed record ConfigurationScopeDeclaredEventSchema(
    Guid AggregateId,
    string DefinitionId,
    string Classification,
    string? Context);

public sealed record ConfigurationScopeRemovedEventSchema(
    Guid AggregateId);
