using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Record;

public sealed record DocumentRecordClosedEvent(
    DocumentRecordId RecordId,
    RecordClosureReason Reason,
    Timestamp ClosedAt) : DomainEvent;
