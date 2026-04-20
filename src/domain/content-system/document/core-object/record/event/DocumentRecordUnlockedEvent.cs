using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Record;

public sealed record DocumentRecordUnlockedEvent(
    DocumentRecordId RecordId,
    Timestamp UnlockedAt) : DomainEvent;
