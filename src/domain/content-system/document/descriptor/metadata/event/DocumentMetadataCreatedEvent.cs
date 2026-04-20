using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed record DocumentMetadataCreatedEvent(
    DocumentMetadataId MetadataId,
    DocumentRef DocumentRef,
    Timestamp CreatedAt) : DomainEvent;
