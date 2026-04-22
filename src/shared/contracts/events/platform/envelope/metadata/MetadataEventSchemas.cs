namespace Whycespace.Shared.Contracts.Events.Platform.Envelope.Metadata;

public sealed record MessageMetadataSchemaRegisteredEventSchema(
    Guid AggregateId,
    int SchemaVersion,
    IReadOnlyList<string> RequiredFields,
    IReadOnlyList<string> OptionalFields);
