using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public sealed record MediaMetadataEntryRemovedEvent(
    MediaMetadataId MetadataId,
    MediaMetadataKey Key,
    Timestamp RemovedAt) : DomainEvent;
