using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Record;

public sealed record DocumentRecordLockedEvent(
    DocumentRecordId RecordId,
    string Reason,
    Timestamp LockedAt) : DomainEvent;
