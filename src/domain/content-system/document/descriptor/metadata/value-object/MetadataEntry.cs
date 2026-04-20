using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed record MetadataEntry(
    MetadataKey Key,
    MetadataValue Value,
    Timestamp UpdatedAt);
