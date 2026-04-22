namespace Whycespace.Shared.Contracts.Events.Platform.Command.CommandDefinition;

public sealed record CommandDefinedEventSchema(
    Guid AggregateId,
    string TypeName,
    string Version,
    string SchemaId,
    string OwnerClassification,
    string OwnerContext,
    string OwnerDomain);

public sealed record CommandDeprecatedEventSchema(Guid AggregateId);
