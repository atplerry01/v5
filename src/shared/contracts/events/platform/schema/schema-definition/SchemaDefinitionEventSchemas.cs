namespace Whycespace.Shared.Contracts.Events.Platform.Schema.SchemaDefinition;

public sealed record SchemaDefinitionDraftedEventSchema(
    Guid AggregateId,
    string SchemaName,
    int Version,
    string CompatibilityMode);

public sealed record SchemaDefinitionPublishedEventSchema(Guid AggregateId);

public sealed record SchemaDefinitionDeprecatedEventSchema(Guid AggregateId);
