using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.File;

public sealed class DocumentFileAggregate : AggregateRoot
{
    public DocumentFileId DocumentFileId { get; private set; }
    public DocumentRef DocumentRef { get; private set; }
    public DocumentFileStorageRef StorageRef { get; private set; }
    public DocumentFileChecksum DeclaredChecksum { get; private set; }
    public DocumentFileMimeType MimeType { get; private set; }
    public DocumentFileSize Size { get; private set; }
    public DocumentFileIntegrityStatus IntegrityStatus { get; private set; }
    public DocumentFileStatus Status { get; private set; }
    public DocumentFileId? SuccessorFileId { get; private set; }
    public Timestamp RegisteredAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private DocumentFileAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static DocumentFileAggregate Register(
        DocumentFileId documentFileId,
        DocumentRef documentRef,
        DocumentFileStorageRef storageRef,
        DocumentFileChecksum declaredChecksum,
        DocumentFileMimeType mimeType,
        DocumentFileSize size,
        Timestamp registeredAt)
    {
        var aggregate = new DocumentFileAggregate();

        aggregate.RaiseDomainEvent(new DocumentFileRegisteredEvent(
            documentFileId,
            documentRef,
            storageRef,
            declaredChecksum,
            mimeType,
            size,
            registeredAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void VerifyIntegrity(DocumentFileChecksum computedChecksum, Timestamp verifiedAt)
    {
        if (Status == DocumentFileStatus.Archived)
            throw DocumentFileErrors.FileArchived();

        if (Status == DocumentFileStatus.Superseded)
            throw DocumentFileErrors.FileAlreadySuperseded();

        if (IntegrityStatus == DocumentFileIntegrityStatus.Verified)
            throw DocumentFileErrors.FileAlreadyVerified();

        if (computedChecksum != DeclaredChecksum)
            throw DocumentFileErrors.ChecksumMismatch(DeclaredChecksum.Value, computedChecksum.Value);

        RaiseDomainEvent(new DocumentFileIntegrityVerifiedEvent(
            DocumentFileId,
            computedChecksum,
            verifiedAt));
    }

    public void Supersede(DocumentFileId successorFileId, Timestamp supersededAt)
    {
        if (Status == DocumentFileStatus.Archived)
            throw DocumentFileErrors.FileArchived();

        if (Status == DocumentFileStatus.Superseded)
            throw DocumentFileErrors.FileAlreadySuperseded();

        if (successorFileId == DocumentFileId)
            throw DocumentFileErrors.CannotSupersedeWithSelf();

        RaiseDomainEvent(new DocumentFileSupersededEvent(DocumentFileId, successorFileId, supersededAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == DocumentFileStatus.Archived)
            throw DocumentFileErrors.AlreadyArchived();

        RaiseDomainEvent(new DocumentFileArchivedEvent(DocumentFileId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DocumentFileRegisteredEvent e:
                DocumentFileId = e.DocumentFileId;
                DocumentRef = e.DocumentRef;
                StorageRef = e.StorageRef;
                DeclaredChecksum = e.DeclaredChecksum;
                MimeType = e.MimeType;
                Size = e.Size;
                IntegrityStatus = DocumentFileIntegrityStatus.Unverified;
                Status = DocumentFileStatus.Registered;
                RegisteredAt = e.RegisteredAt;
                LastModifiedAt = e.RegisteredAt;
                break;

            case DocumentFileIntegrityVerifiedEvent e:
                IntegrityStatus = DocumentFileIntegrityStatus.Verified;
                LastModifiedAt = e.VerifiedAt;
                break;

            case DocumentFileSupersededEvent e:
                Status = DocumentFileStatus.Superseded;
                SuccessorFileId = e.SuccessorFileId;
                LastModifiedAt = e.SupersededAt;
                break;

            case DocumentFileArchivedEvent e:
                Status = DocumentFileStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DocumentRef.Value == Guid.Empty)
            throw DocumentFileErrors.OrphanedDocumentFile();
    }
}
