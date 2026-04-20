using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public sealed class MediaFileAggregate : AggregateRoot
{
    public MediaFileId MediaFileId { get; private set; }
    public StorageReference StorageReference { get; private set; }
    public MediaChecksum DeclaredChecksum { get; private set; }
    public MediaMimeType MimeType { get; private set; }
    public FileSize Size { get; private set; }
    public FileIntegrityStatus IntegrityStatus { get; private set; }
    public FileRegistrationStatus RegistrationStatus { get; private set; }
    public MediaFileId? SuccessorFileId { get; private set; }
    public Timestamp RegisteredAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private MediaFileAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static MediaFileAggregate Register(
        MediaFileId mediaFileId,
        StorageReference storageReference,
        MediaChecksum declaredChecksum,
        MediaMimeType mimeType,
        FileSize size,
        Timestamp registeredAt)
    {
        var aggregate = new MediaFileAggregate();

        aggregate.RaiseDomainEvent(new MediaFileRegisteredEvent(
            mediaFileId,
            storageReference,
            declaredChecksum,
            mimeType,
            size,
            registeredAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void VerifyIntegrity(MediaChecksum computedChecksum, Timestamp verifiedAt)
    {
        if (RegistrationStatus == FileRegistrationStatus.Superseded)
            throw MediaFileErrors.FileAlreadySuperseded();

        if (IntegrityStatus == FileIntegrityStatus.Verified)
            throw MediaFileErrors.FileAlreadyVerified();

        if (IntegrityStatus == FileIntegrityStatus.Corrupt)
            throw MediaFileErrors.FileAlreadyCorrupt();

        if (computedChecksum != DeclaredChecksum)
            throw MediaFileErrors.ChecksumMismatch(DeclaredChecksum.Value, computedChecksum.Value);

        RaiseDomainEvent(new MediaFileIntegrityVerifiedEvent(
            MediaFileId,
            computedChecksum,
            verifiedAt));
    }

    public void MarkCorrupt(string reason, Timestamp markedAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw MediaFileErrors.InvalidFailureReason();

        if (IntegrityStatus == FileIntegrityStatus.Corrupt)
            throw MediaFileErrors.FileAlreadyCorrupt();

        RaiseDomainEvent(new MediaFileMarkedCorruptEvent(MediaFileId, reason.Trim(), markedAt));
    }

    public void Supersede(MediaFileId successorFileId, Timestamp supersededAt)
    {
        if (RegistrationStatus == FileRegistrationStatus.Superseded)
            throw MediaFileErrors.FileAlreadySuperseded();

        if (successorFileId == MediaFileId)
            throw MediaFileErrors.CannotSupersedeWithSelf();

        RaiseDomainEvent(new MediaFileSupersededEvent(MediaFileId, successorFileId, supersededAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MediaFileRegisteredEvent e:
                MediaFileId = e.MediaFileId;
                StorageReference = e.StorageReference;
                DeclaredChecksum = e.DeclaredChecksum;
                MimeType = e.MimeType;
                Size = e.Size;
                IntegrityStatus = FileIntegrityStatus.Unverified;
                RegistrationStatus = FileRegistrationStatus.Registered;
                RegisteredAt = e.RegisteredAt;
                LastModifiedAt = e.RegisteredAt;
                break;

            case MediaFileIntegrityVerifiedEvent e:
                IntegrityStatus = FileIntegrityStatus.Verified;
                LastModifiedAt = e.VerifiedAt;
                break;

            case MediaFileMarkedCorruptEvent e:
                IntegrityStatus = FileIntegrityStatus.Corrupt;
                LastModifiedAt = e.MarkedAt;
                break;

            case MediaFileSupersededEvent e:
                RegistrationStatus = FileRegistrationStatus.Superseded;
                SuccessorFileId = e.SuccessorFileId;
                LastModifiedAt = e.SupersededAt;
                break;
        }
    }
}
