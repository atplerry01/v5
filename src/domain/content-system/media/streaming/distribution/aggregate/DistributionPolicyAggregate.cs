using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Distribution;

public sealed class DistributionPolicyAggregate : AggregateRoot
{
    private static readonly DistributionSpecification Spec = new();
    private readonly HashSet<string> _channels = new(StringComparer.Ordinal);

    public DistributionPolicyId PolicyId { get; private set; }
    public string AssetRef { get; private set; } = string.Empty;
    public DistributionStatus Status { get; private set; }
    public Timestamp AttachedAt { get; private set; }
    public IReadOnlyCollection<string> Channels => _channels;

    private DistributionPolicyAggregate() { }

    public static DistributionPolicyAggregate Attach(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        DistributionPolicyId id, string assetRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(assetRef)) throw DistributionErrors.InvalidAssetRef();
        var agg = new DistributionPolicyAggregate();
        agg.RaiseDomainEvent(new DistributionPolicyAttachedEvent(eventId, aggregateId, correlationId, causationId, id, assetRef, at));
        return agg;
    }

    public void AddChannel(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, DistributionChannel channel, Timestamp at)
    {
        Spec.EnsureActive(Status);
        if (_channels.Contains(channel.Name))
            throw DistributionErrors.ChannelAlreadyPresent(channel.Name);
        RaiseDomainEvent(new DistributionChannelAddedEvent(eventId, aggregateId, correlationId, causationId, PolicyId, channel.Name, at));
    }

    public void RemoveChannel(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, DistributionChannel channel, Timestamp at)
    {
        Spec.EnsureActive(Status);
        if (!_channels.Contains(channel.Name))
            throw DistributionErrors.ChannelNotPresent(channel.Name);
        RaiseDomainEvent(new DistributionChannelRemovedEvent(eventId, aggregateId, correlationId, causationId, PolicyId, channel.Name, at));
    }

    public void Deactivate(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == DistributionStatus.Deactivated) throw DistributionErrors.AlreadyDeactivated();
        RaiseDomainEvent(new DistributionPolicyDeactivatedEvent(eventId, aggregateId, correlationId, causationId, PolicyId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DistributionPolicyAttachedEvent e:
                PolicyId = e.PolicyId;
                AssetRef = e.AssetRef;
                Status = DistributionStatus.Active;
                AttachedAt = e.AttachedAt;
                break;
            case DistributionChannelAddedEvent e: _channels.Add(e.Channel); break;
            case DistributionChannelRemovedEvent e: _channels.Remove(e.Channel); break;
            case DistributionPolicyDeactivatedEvent: Status = DistributionStatus.Deactivated; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(AssetRef))
            throw DistributionErrors.AssetMissing();
    }
}
