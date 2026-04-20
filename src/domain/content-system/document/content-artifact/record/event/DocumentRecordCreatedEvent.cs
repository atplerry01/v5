using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Record;

public sealed record DocumentRecordCreatedEvent(
    DocumentRecordId RecordId,
    DocumentRef DocumentRef,
    Timestamp CreatedAt) : DomainEvent;
