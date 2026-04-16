using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Version;

public sealed class AssetVersionAggregate : AggregateRoot
{
    private static readonly AssetVersionSpecification Spec = new();

    public AssetVersionId VersionId { get; private set; }
    public string AssetRef { get; private set; } = string.Empty;
    public VersionNumber Number { get; private set; } = default!;
    public AssetVersionStatus Status { get; private set; }
    public AssetVersionId? SupersededBy { get; private set; }
    public Timestamp DraftedAt { get; private set; }

    private AssetVersionAggregate() { }

    public static AssetVersionAggregate Draft(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        AssetVersionId id, string assetRef, VersionNumber number, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(assetRef)) throw AssetVersionErrors.InvalidAssetRef();
        var agg = new AssetVersionAggregate();
        agg.RaiseDomainEvent(new AssetVersionDraftedEvent(
            eventId, aggregateId, correlationId, causationId, id, assetRef,
            number.Major, number.Minor, number.Patch, at));
        return agg;
    }

    public void Promote(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsurePromotable(Status);
        RaiseDomainEvent(new AssetVersionPromotedEvent(eventId, aggregateId, correlationId, causationId, VersionId, at));
    }

    public void Supersede(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, AssetVersionId successor, Timestamp at)
    {
        Spec.EnsureSupersedable(Status);
        if (successor.Value == Guid.Empty) throw AssetVersionErrors.InvalidVersionNumber();
        RaiseDomainEvent(new AssetVersionSupersededEvent(eventId, aggregateId, correlationId, causationId, VersionId, successor, at));
    }

    public void Retire(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsureRetirable(Status);
        RaiseDomainEvent(new AssetVersionRetiredEvent(eventId, aggregateId, correlationId, causationId, VersionId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AssetVersionDraftedEvent e:
                VersionId = e.VersionId;
                AssetRef = e.AssetRef;
                Number = VersionNumber.Create(e.Major, e.Minor, e.Patch);
                Status = AssetVersionStatus.Draft;
                DraftedAt = e.DraftedAt;
                break;
            case AssetVersionPromotedEvent: Status = AssetVersionStatus.Promoted; break;
            case AssetVersionSupersededEvent e:
                Status = AssetVersionStatus.Superseded;
                SupersededBy = e.SucceededBy;
                break;
            case AssetVersionRetiredEvent: Status = AssetVersionStatus.Retired; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Number is null) return;
        if (string.IsNullOrEmpty(AssetRef))
            throw AssetVersionErrors.AssetMissing();
    }
}
