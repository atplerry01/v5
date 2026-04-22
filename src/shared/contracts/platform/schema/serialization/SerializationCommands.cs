using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Schema.Serialization;

public sealed record RegisterSerializationFormatCommand(
    Guid SerializationFormatId,
    string FormatName,
    string Encoding,
    Guid? SchemaRef,
    IReadOnlyList<SerializationOptionDto> Options,
    string RoundTripGuarantee,
    int FormatVersion,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => SerializationFormatId;
}

public sealed record DeprecateSerializationFormatCommand(
    Guid SerializationFormatId,
    DateTimeOffset DeprecatedAt) : IHasAggregateId
{
    public Guid AggregateId => SerializationFormatId;
}

public sealed record SerializationOptionDto(string Key, string Value);
