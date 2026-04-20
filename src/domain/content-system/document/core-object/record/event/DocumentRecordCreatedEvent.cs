using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Record;

public sealed record DocumentRecordCreatedEvent(
    DocumentRecordId RecordId,
    DocumentRef DocumentRef,
    Timestamp CreatedAt) : DomainEvent;
