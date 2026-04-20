using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Record;

public sealed record DocumentRecordClosedEvent(
    DocumentRecordId RecordId,
    RecordClosureReason Reason,
    Timestamp ClosedAt) : DomainEvent;
