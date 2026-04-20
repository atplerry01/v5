using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public sealed record DocumentArchivedEvent(
    DocumentId DocumentId,
    Timestamp ArchivedAt) : DomainEvent;
