namespace Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationDefinition;

public sealed record ConfigurationDefinedEventSchema(
    Guid AggregateId,
    string Name,
    string ValueType,
    string Description,
    string? DefaultValue);

public sealed record ConfigurationDefinitionDeprecatedEventSchema(
    Guid AggregateId);
