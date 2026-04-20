using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public sealed record MediaMetadataEntryUpdatedEvent(
    MediaMetadataId MetadataId,
    MediaMetadataKey Key,
    MediaMetadataValue PreviousValue,
    MediaMetadataValue NewValue,
    Timestamp UpdatedAt) : DomainEvent;
