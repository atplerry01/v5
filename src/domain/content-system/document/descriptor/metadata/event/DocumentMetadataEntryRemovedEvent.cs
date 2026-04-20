using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed record DocumentMetadataEntryRemovedEvent(
    DocumentMetadataId MetadataId,
    MetadataKey Key,
    Timestamp RemovedAt) : DomainEvent;
