using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Asset;

public sealed class AssetAggregate : AggregateRoot
{
    public AssetId AssetId { get; private set; }
    public AssetTitle Title { get; private set; }
    public AssetClassification Classification { get; private set; }
    public AssetStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private AssetAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static AssetAggregate Create(
        AssetId assetId,
        AssetTitle title,
        AssetClassification classification,
        Timestamp createdAt)
    {
        var aggregate = new AssetAggregate();

        aggregate.RaiseDomainEvent(new AssetCreatedEvent(
            assetId,
            title,
            classification,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Rename(AssetTitle newTitle, Timestamp renamedAt)
    {
        if (Status == AssetStatus.Retired)
            throw AssetErrors.CannotModifyRetiredAsset();

        if (Title == newTitle)
            return;

        RaiseDomainEvent(new AssetRenamedEvent(AssetId, Title, newTitle, renamedAt));
    }

    public void Reclassify(AssetClassification newClassification, Timestamp reclassifiedAt)
    {
        if (Status == AssetStatus.Retired)
            throw AssetErrors.CannotModifyRetiredAsset();

        if (Classification == newClassification)
            return;

        RaiseDomainEvent(new AssetReclassifiedEvent(
            AssetId,
            Classification,
            newClassification,
            reclassifiedAt));
    }

    public void Activate(Timestamp activatedAt)
    {
        if (Status == AssetStatus.Retired)
            throw AssetErrors.CannotModifyRetiredAsset();

        if (Status == AssetStatus.Active)
            throw AssetErrors.AssetAlreadyActive();

        RaiseDomainEvent(new AssetActivatedEvent(AssetId, activatedAt));
    }

    public void Retire(Timestamp retiredAt)
    {
        if (Status == AssetStatus.Retired)
            throw AssetErrors.AssetAlreadyRetired();

        RaiseDomainEvent(new AssetRetiredEvent(AssetId, retiredAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AssetCreatedEvent e:
                AssetId = e.AssetId;
                Title = e.Title;
                Classification = e.Classification;
                Status = AssetStatus.Draft;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case AssetRenamedEvent e:
                Title = e.NewTitle;
                LastModifiedAt = e.RenamedAt;
                break;

            case AssetReclassifiedEvent e:
                Classification = e.NewClassification;
                LastModifiedAt = e.ReclassifiedAt;
                break;

            case AssetActivatedEvent e:
                Status = AssetStatus.Active;
                LastModifiedAt = e.ActivatedAt;
                break;

            case AssetRetiredEvent e:
                Status = AssetStatus.Retired;
                LastModifiedAt = e.RetiredAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Status == AssetStatus.Active && Classification == AssetClassification.Unclassified)
            throw AssetErrors.MissingClassification();
    }
}
