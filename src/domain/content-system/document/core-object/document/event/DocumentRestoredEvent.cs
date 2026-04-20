using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public sealed record DocumentRestoredEvent(
    DocumentId DocumentId,
    Timestamp RestoredAt) : DomainEvent;
