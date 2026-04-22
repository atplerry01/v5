using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Serialization;

public sealed record SerializationFormatRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] SerializationFormatId SerializationFormatId,
    string FormatName,
    SerializationEncoding Encoding,
    Guid? SchemaRef,
    IReadOnlyList<SerializationOption> Options,
    RoundTripGuarantee RoundTripGuarantee,
    int FormatVersion,
    Timestamp RegisteredAt) : DomainEvent;
