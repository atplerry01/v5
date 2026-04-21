using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public sealed record MediaMetadataEntryAddedEvent(
    [property: JsonPropertyName("AggregateId")] MediaMetadataId MetadataId,
    MediaMetadataKey Key,
    MediaMetadataValue Value,
    Timestamp AddedAt) : DomainEvent;
