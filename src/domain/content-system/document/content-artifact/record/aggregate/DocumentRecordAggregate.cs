using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Record;

public sealed class DocumentRecordAggregate : AggregateRoot
{
    public DocumentRecordId RecordId { get; private set; }
    public DocumentRef DocumentRef { get; private set; }
    public RecordStatus Status { get; private set; }
    public RecordClosureReason? ClosureReason { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp? ClosedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private DocumentRecordAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static DocumentRecordAggregate Create(
        DocumentRecordId recordId,
        DocumentRef documentRef,
        Timestamp createdAt)
    {
        var aggregate = new DocumentRecordAggregate();

        aggregate.RaiseDomainEvent(new DocumentRecordCreatedEvent(recordId, documentRef, createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Lock(string reason, Timestamp lockedAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw DocumentRecordErrors.InvalidLockReason();

        if (Status == RecordStatus.Archived)
            throw DocumentRecordErrors.RecordArchived();

        if (Status == RecordStatus.Closed)
            throw DocumentRecordErrors.RecordClosed();

        if (Status == RecordStatus.Locked)
            throw DocumentRecordErrors.AlreadyLocked();

        RaiseDomainEvent(new DocumentRecordLockedEvent(RecordId, reason.Trim(), lockedAt));
    }

    public void Unlock(Timestamp unlockedAt)
    {
        if (Status == RecordStatus.Archived)
            throw DocumentRecordErrors.RecordArchived();

        if (Status == RecordStatus.Closed)
            throw DocumentRecordErrors.RecordClosed();

        if (Status != RecordStatus.Locked)
            throw DocumentRecordErrors.NotLocked();

        RaiseDomainEvent(new DocumentRecordUnlockedEvent(RecordId, unlockedAt));
    }

    public void Close(RecordClosureReason reason, Timestamp closedAt)
    {
        if (Status == RecordStatus.Archived)
            throw DocumentRecordErrors.RecordArchived();

        if (Status == RecordStatus.Closed)
            throw DocumentRecordErrors.AlreadyClosed();

        RaiseDomainEvent(new DocumentRecordClosedEvent(RecordId, reason, closedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == RecordStatus.Archived)
            throw DocumentRecordErrors.AlreadyArchived();

        RaiseDomainEvent(new DocumentRecordArchivedEvent(RecordId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DocumentRecordCreatedEvent e:
                RecordId = e.RecordId;
                DocumentRef = e.DocumentRef;
                Status = RecordStatus.Open;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case DocumentRecordLockedEvent e:
                Status = RecordStatus.Locked;
                LastModifiedAt = e.LockedAt;
                break;

            case DocumentRecordUnlockedEvent e:
                Status = RecordStatus.Open;
                LastModifiedAt = e.UnlockedAt;
                break;

            case DocumentRecordClosedEvent e:
                Status = RecordStatus.Closed;
                ClosureReason = e.Reason;
                ClosedAt = e.ClosedAt;
                LastModifiedAt = e.ClosedAt;
                break;

            case DocumentRecordArchivedEvent e:
                Status = RecordStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DocumentRef.Value == Guid.Empty)
            throw DocumentRecordErrors.OrphanedRecord();
    }
}
