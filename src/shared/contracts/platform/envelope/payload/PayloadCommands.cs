using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Envelope.Payload;

public sealed record RegisterPayloadSchemaCommand(
    Guid PayloadSchemaId,
    string TypeRef,
    string Encoding,
    string? SchemaRef,
    int SchemaContractVersion,
    long? MaxSizeBytes,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => PayloadSchemaId;
}

public sealed record DeprecatePayloadSchemaCommand(
    Guid PayloadSchemaId,
    DateTimeOffset DeprecatedAt) : IHasAggregateId
{
    public Guid AggregateId => PayloadSchemaId;
}
