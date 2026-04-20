using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed record DocumentMetadataFinalizedEvent(
    DocumentMetadataId MetadataId,
    Timestamp FinalizedAt) : DomainEvent;
