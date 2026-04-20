using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Version;

public sealed class DocumentVersionAggregate : AggregateRoot
{
    public DocumentVersionId VersionId { get; private set; }
    public DocumentRef DocumentRef { get; private set; }
    public VersionNumber VersionNumber { get; private set; }
    public ArtifactRef ArtifactRef { get; private set; }
    public DocumentVersionId? PreviousVersionId { get; private set; }
    public DocumentVersionId? SuccessorVersionId { get; private set; }
    public VersionStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private DocumentVersionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static DocumentVersionAggregate Create(
        DocumentVersionId versionId,
        DocumentRef documentRef,
        VersionNumber versionNumber,
        ArtifactRef artifactRef,
        DocumentVersionId? previousVersionId,
        Timestamp createdAt)
    {
        var aggregate = new DocumentVersionAggregate();

        aggregate.RaiseDomainEvent(new DocumentVersionCreatedEvent(
            versionId,
            documentRef,
            versionNumber,
            artifactRef,
            previousVersionId,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Activate(Timestamp activatedAt)
    {
        if (Status == VersionStatus.Active)
            throw DocumentVersionErrors.VersionAlreadyActive();

        if (Status != VersionStatus.Draft)
            throw DocumentVersionErrors.VersionNotDraft();

        RaiseDomainEvent(new DocumentVersionActivatedEvent(VersionId, activatedAt));
    }

    public void Supersede(DocumentVersionId successorVersionId, Timestamp supersededAt)
    {
        if (Status == VersionStatus.Superseded)
            throw DocumentVersionErrors.VersionAlreadySuperseded();

        if (Status != VersionStatus.Active)
            throw DocumentVersionErrors.CannotSupersedeNonActive();

        if (successorVersionId == VersionId)
            throw DocumentVersionErrors.CannotSupersedeWithSelf();

        RaiseDomainEvent(new DocumentVersionSupersededEvent(VersionId, successorVersionId, supersededAt));
    }

    public void Withdraw(string reason, Timestamp withdrawnAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw DocumentVersionErrors.InvalidWithdrawalReason();

        if (Status == VersionStatus.Withdrawn)
            throw DocumentVersionErrors.VersionAlreadyWithdrawn();

        RaiseDomainEvent(new DocumentVersionWithdrawnEvent(VersionId, reason.Trim(), withdrawnAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DocumentVersionCreatedEvent e:
                VersionId = e.VersionId;
                DocumentRef = e.DocumentRef;
                VersionNumber = e.VersionNumber;
                ArtifactRef = e.ArtifactRef;
                PreviousVersionId = e.PreviousVersionId;
                Status = VersionStatus.Draft;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case DocumentVersionActivatedEvent e:
                Status = VersionStatus.Active;
                LastModifiedAt = e.ActivatedAt;
                break;

            case DocumentVersionSupersededEvent e:
                Status = VersionStatus.Superseded;
                SuccessorVersionId = e.SuccessorVersionId;
                LastModifiedAt = e.SupersededAt;
                break;

            case DocumentVersionWithdrawnEvent e:
                Status = VersionStatus.Withdrawn;
                LastModifiedAt = e.WithdrawnAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DocumentRef.Value == Guid.Empty)
            throw DocumentVersionErrors.OrphanedVersion();
    }
}
