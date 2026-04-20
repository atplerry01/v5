using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Manifest;

public sealed class ManifestAggregate : AggregateRoot
{
    public ManifestId ManifestId { get; private set; }
    public ManifestSourceRef SourceRef { get; private set; }
    public ManifestVersion CurrentVersion { get; private set; }
    public ManifestStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp? PublishedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private ManifestAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ManifestAggregate Create(
        ManifestId manifestId,
        ManifestSourceRef sourceRef,
        Timestamp createdAt)
    {
        var aggregate = new ManifestAggregate();

        aggregate.RaiseDomainEvent(new ManifestCreatedEvent(
            manifestId,
            sourceRef,
            new ManifestVersion(1),
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Publish(Timestamp publishedAt)
    {
        if (Status == ManifestStatus.Archived)
            throw ManifestErrors.ManifestArchived();

        if (Status == ManifestStatus.Retired)
            throw ManifestErrors.ManifestRetired();

        if (Status == ManifestStatus.Published)
            throw ManifestErrors.AlreadyPublished();

        RaiseDomainEvent(new ManifestPublishedEvent(ManifestId, CurrentVersion, publishedAt));
    }

    public void Update(Timestamp updatedAt)
    {
        if (Status == ManifestStatus.Archived)
            throw ManifestErrors.ManifestArchived();

        if (Status == ManifestStatus.Retired)
            throw ManifestErrors.ManifestRetired();

        if (Status != ManifestStatus.Published)
            throw ManifestErrors.CannotUpdateUnpublished();

        var next = CurrentVersion.Next();
        RaiseDomainEvent(new ManifestUpdatedEvent(ManifestId, CurrentVersion, next, updatedAt));
    }

    public void Retire(string reason, Timestamp retiredAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ManifestErrors.InvalidRetirementReason();

        if (Status == ManifestStatus.Archived)
            throw ManifestErrors.ManifestArchived();

        if (Status == ManifestStatus.Retired)
            throw ManifestErrors.AlreadyRetired();

        RaiseDomainEvent(new ManifestRetiredEvent(ManifestId, reason.Trim(), retiredAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == ManifestStatus.Archived)
            throw ManifestErrors.AlreadyArchived();

        RaiseDomainEvent(new ManifestArchivedEvent(ManifestId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ManifestCreatedEvent e:
                ManifestId = e.ManifestId;
                SourceRef = e.SourceRef;
                CurrentVersion = e.InitialVersion;
                Status = ManifestStatus.Created;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case ManifestPublishedEvent e:
                Status = ManifestStatus.Published;
                PublishedAt ??= e.PublishedAt;
                LastModifiedAt = e.PublishedAt;
                break;

            case ManifestUpdatedEvent e:
                CurrentVersion = e.NewVersion;
                LastModifiedAt = e.UpdatedAt;
                break;

            case ManifestRetiredEvent e:
                Status = ManifestStatus.Retired;
                LastModifiedAt = e.RetiredAt;
                break;

            case ManifestArchivedEvent e:
                Status = ManifestStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (SourceRef.Value == Guid.Empty)
            throw ManifestErrors.OrphanedManifest();
    }
}
