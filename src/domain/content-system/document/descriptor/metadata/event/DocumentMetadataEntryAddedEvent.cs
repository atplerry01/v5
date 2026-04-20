using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed record DocumentMetadataEntryAddedEvent(
    DocumentMetadataId MetadataId,
    MetadataKey Key,
    MetadataValue Value,
    Timestamp AddedAt) : DomainEvent;
