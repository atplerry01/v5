using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public sealed record MediaMetadataFinalizedEvent(
    [property: JsonPropertyName("AggregateId")] MediaMetadataId MetadataId,
    Timestamp FinalizedAt) : DomainEvent;
