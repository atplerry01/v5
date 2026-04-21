using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public sealed record MediaMetadataCreatedEvent(
    [property: JsonPropertyName("AggregateId")] MediaMetadataId MetadataId,
    MediaAssetRef AssetRef,
    Timestamp CreatedAt) : DomainEvent;
