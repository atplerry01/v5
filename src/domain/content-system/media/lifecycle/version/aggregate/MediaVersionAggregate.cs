using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Version;

public sealed class MediaVersionAggregate : AggregateRoot
{
    public MediaVersionId VersionId { get; private set; }
    public MediaAssetRef AssetRef { get; private set; }
    public MediaVersionNumber VersionNumber { get; private set; }
    public MediaFileRef FileRef { get; private set; }
    public MediaVersionId? PreviousVersionId { get; private set; }
    public MediaVersionId? SuccessorVersionId { get; private set; }
    public MediaVersionStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private MediaVersionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static MediaVersionAggregate Create(
        MediaVersionId versionId,
        MediaAssetRef assetRef,
        MediaVersionNumber versionNumber,
        MediaFileRef fileRef,
        MediaVersionId? previousVersionId,
        Timestamp createdAt)
    {
        var aggregate = new MediaVersionAggregate();

        aggregate.RaiseDomainEvent(new MediaVersionCreatedEvent(
            versionId,
            assetRef,
            versionNumber,
            fileRef,
            previousVersionId,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Activate(Timestamp activatedAt)
    {
        if (Status == MediaVersionStatus.Active)
            throw MediaVersionErrors.VersionAlreadyActive();

        if (Status != MediaVersionStatus.Draft)
            throw MediaVersionErrors.VersionNotDraft();

        RaiseDomainEvent(new MediaVersionActivatedEvent(VersionId, activatedAt));
    }

    public void Supersede(MediaVersionId successorVersionId, Timestamp supersededAt)
    {
        if (Status == MediaVersionStatus.Superseded)
            throw MediaVersionErrors.VersionAlreadySuperseded();

        if (Status != MediaVersionStatus.Active)
            throw MediaVersionErrors.CannotSupersedeNonActive();

        if (successorVersionId == VersionId)
            throw MediaVersionErrors.CannotSupersedeWithSelf();

        RaiseDomainEvent(new MediaVersionSupersededEvent(VersionId, successorVersionId, supersededAt));
    }

    public void Withdraw(string reason, Timestamp withdrawnAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw MediaVersionErrors.InvalidWithdrawalReason();

        if (Status == MediaVersionStatus.Withdrawn)
            throw MediaVersionErrors.VersionAlreadyWithdrawn();

        RaiseDomainEvent(new MediaVersionWithdrawnEvent(VersionId, reason.Trim(), withdrawnAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MediaVersionCreatedEvent e:
                VersionId = e.VersionId;
                AssetRef = e.AssetRef;
                VersionNumber = e.VersionNumber;
                FileRef = e.FileRef;
                PreviousVersionId = e.PreviousVersionId;
                Status = MediaVersionStatus.Draft;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case MediaVersionActivatedEvent e:
                Status = MediaVersionStatus.Active;
                LastModifiedAt = e.ActivatedAt;
                break;

            case MediaVersionSupersededEvent e:
                Status = MediaVersionStatus.Superseded;
                SuccessorVersionId = e.SuccessorVersionId;
                LastModifiedAt = e.SupersededAt;
                break;

            case MediaVersionWithdrawnEvent e:
                Status = MediaVersionStatus.Withdrawn;
                LastModifiedAt = e.WithdrawnAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (AssetRef.Value == Guid.Empty)
            throw MediaVersionErrors.OrphanedMediaVersion();
    }
}
