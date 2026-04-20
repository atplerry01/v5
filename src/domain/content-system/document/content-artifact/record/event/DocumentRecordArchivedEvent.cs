using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Record;

public sealed record DocumentRecordArchivedEvent(
    DocumentRecordId RecordId,
    Timestamp ArchivedAt) : DomainEvent;
