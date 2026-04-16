using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Feed;

public sealed class FeedAggregate : AggregateRoot
{
    private readonly Dictionary<string, FeedItem> _items = new(StringComparer.Ordinal);
    private readonly HashSet<string> _pinned = new(StringComparer.Ordinal);

    public FeedId FeedId { get; private set; }
    public string OwnerRef { get; private set; } = string.Empty;
    public Timestamp CreatedAt { get; private set; }
    public IReadOnlyCollection<FeedItem> Items => _items.Values;
    public IReadOnlyCollection<string> Pinned => _pinned;

    private FeedAggregate() { }

    public static FeedAggregate Create(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        FeedId id, string ownerRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(ownerRef)) throw FeedErrors.InvalidOwner();
        var agg = new FeedAggregate();
        agg.RaiseDomainEvent(new FeedCreatedEvent(eventId, aggregateId, correlationId, causationId, id, ownerRef, at));
        return agg;
    }

    public void Append(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string itemRef, int rank, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(itemRef)) throw FeedErrors.InvalidItem();
        if (rank < 0) throw FeedErrors.InvalidRank();
        if (_items.ContainsKey(itemRef)) throw FeedErrors.ItemAlreadyPresent(itemRef);
        RaiseDomainEvent(new FeedItemAppendedEvent(eventId, aggregateId, correlationId, causationId, FeedId, itemRef, rank, at));
    }

    public void Pin(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string itemRef, Timestamp at)
    {
        if (!_items.ContainsKey(itemRef)) throw FeedErrors.ItemNotPresent(itemRef);
        RaiseDomainEvent(new FeedItemPinnedEvent(eventId, aggregateId, correlationId, causationId, FeedId, itemRef, at));
    }

    public void Clear(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        RaiseDomainEvent(new FeedClearedEvent(eventId, aggregateId, correlationId, causationId, FeedId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case FeedCreatedEvent e:
                FeedId = e.FeedId;
                OwnerRef = e.OwnerRef;
                CreatedAt = e.CreatedAt;
                break;
            case FeedItemAppendedEvent e:
                _items[e.ItemRef] = FeedItem.Create(e.ItemRef, e.Rank, e.AppendedAt);
                break;
            case FeedItemPinnedEvent e: _pinned.Add(e.ItemRef); break;
            case FeedClearedEvent:
                _items.Clear();
                _pinned.Clear();
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(OwnerRef))
            throw FeedErrors.OwnerMissing();
    }
}
