using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public sealed record MediaMetadataEntry(
    MediaMetadataKey Key,
    MediaMetadataValue Value,
    Timestamp UpdatedAt);
