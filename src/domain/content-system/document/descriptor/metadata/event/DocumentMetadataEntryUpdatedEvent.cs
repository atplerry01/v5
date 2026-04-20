using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed record DocumentMetadataEntryUpdatedEvent(
    DocumentMetadataId MetadataId,
    MetadataKey Key,
    MetadataValue PreviousValue,
    MetadataValue NewValue,
    Timestamp UpdatedAt) : DomainEvent;
