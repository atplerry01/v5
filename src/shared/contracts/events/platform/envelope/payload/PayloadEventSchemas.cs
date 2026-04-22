namespace Whycespace.Shared.Contracts.Events.Platform.Envelope.Payload;

public sealed record PayloadSchemaRegisteredEventSchema(
    Guid AggregateId,
    string TypeRef,
    string Encoding,
    string? SchemaRef,
    int SchemaContractVersion,
    long? MaxSizeBytes);

public sealed record PayloadSchemaDeprecatedEventSchema(Guid AggregateId);
