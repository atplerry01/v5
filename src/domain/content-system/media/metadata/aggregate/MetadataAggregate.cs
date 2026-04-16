using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Metadata;

public sealed class MetadataAggregate : AggregateRoot
{
    private static readonly MetadataSpecification Spec = new();
    private readonly Dictionary<string, string> _fields = new(StringComparer.Ordinal);
    private readonly HashSet<string> _tags = new(StringComparer.Ordinal);

    public MetadataId MetadataId { get; private set; }
    public string AssetRef { get; private set; } = string.Empty;
    public MetadataStatus Status { get; private set; }
    public Timestamp AttachedAt { get; private set; }
    public IReadOnlyDictionary<string, string> Fields => _fields;
    public IReadOnlyCollection<string> Tags => _tags;

    private MetadataAggregate() { }

    public static MetadataAggregate Attach(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        MetadataId id, string assetRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(assetRef)) throw MetadataErrors.InvalidAssetRef();
        var agg = new MetadataAggregate();
        agg.RaiseDomainEvent(new MetadataAttachedEvent(eventId, aggregateId, correlationId, causationId, id, assetRef, at));
        return agg;
    }

    public void SetField(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, MetadataField field, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        RaiseDomainEvent(new MetadataFieldUpdatedEvent(eventId, aggregateId, correlationId, causationId, MetadataId, field.Key, field.Value, at));
    }

    public void AddTag(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string tag, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        var normalised = (tag ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalised)) throw MetadataErrors.InvalidKey();
        if (_tags.Contains(normalised)) throw MetadataErrors.TagAlreadyPresent(normalised);
        RaiseDomainEvent(new MetadataTaggedEvent(eventId, aggregateId, correlationId, causationId, MetadataId, normalised, at));
    }

    public void Lock(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == MetadataStatus.Locked) throw MetadataErrors.AlreadyLocked();
        RaiseDomainEvent(new MetadataLockedEvent(eventId, aggregateId, correlationId, causationId, MetadataId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MetadataAttachedEvent e:
                MetadataId = e.MetadataId;
                AssetRef = e.AssetRef;
                Status = MetadataStatus.Attached;
                AttachedAt = e.AttachedAt;
                break;
            case MetadataFieldUpdatedEvent e: _fields[e.Key] = e.Value; break;
            case MetadataTaggedEvent e: _tags.Add(e.Tag); break;
            case MetadataLockedEvent: Status = MetadataStatus.Locked; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Status == MetadataStatus.Attached && string.IsNullOrEmpty(AssetRef) && DomainEvents.Count > 0)
            throw MetadataErrors.AssetMissing();
    }
}
