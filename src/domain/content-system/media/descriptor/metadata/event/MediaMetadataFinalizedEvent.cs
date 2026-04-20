using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public sealed record MediaMetadataFinalizedEvent(
    MediaMetadataId MetadataId,
    Timestamp FinalizedAt) : DomainEvent;
