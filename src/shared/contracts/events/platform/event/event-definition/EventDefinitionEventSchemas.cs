namespace Whycespace.Shared.Contracts.Events.Platform.Event.EventDefinition;

public sealed record EventDefinedEventSchema(
    Guid AggregateId,
    string TypeName,
    string Version,
    string SchemaId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain);

public sealed record EventDefinitionDeprecatedEventSchema(Guid AggregateId);
