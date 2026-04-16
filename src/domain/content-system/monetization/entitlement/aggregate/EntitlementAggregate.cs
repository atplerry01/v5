using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Entitlement;

public sealed class EntitlementAggregate : AggregateRoot
{
    private static readonly EntitlementSpecification Spec = new();
    private readonly Dictionary<string, EntitlementScope> _scopes = new(StringComparer.Ordinal);

    public EntitlementId EntitlementId { get; private set; }
    public string HolderRef { get; private set; } = string.Empty;
    public EntitlementTier Tier { get; private set; }
    public EntitlementStatus Status { get; private set; }
    public Timestamp ValidFrom { get; private set; }
    public Timestamp ValidUntil { get; private set; }
    public IReadOnlyCollection<EntitlementScope> Scopes => _scopes.Values;

    private EntitlementAggregate() { }

    public static EntitlementAggregate Grant(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        EntitlementId id, string holderRef, EntitlementTier tier, Timestamp validFrom, Timestamp validUntil, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(holderRef)) throw EntitlementErrors.InvalidHolder();
        Spec.EnsureValidity(validFrom, validUntil);
        var agg = new EntitlementAggregate();
        agg.RaiseDomainEvent(new EntitlementGrantedEvent(
            eventId, aggregateId, correlationId, causationId, id, holderRef, tier, validFrom, validUntil, at));
        return agg;
    }

    public void Extend(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp newValidUntil, Timestamp at)
    {
        if (Status == EntitlementStatus.Revoked) throw EntitlementErrors.CannotExtendRevoked();
        if (newValidUntil.Value <= ValidUntil.Value) throw EntitlementErrors.InvalidValidity();
        RaiseDomainEvent(new EntitlementExtendedEvent(eventId, aggregateId, correlationId, causationId, EntitlementId, newValidUntil, at));
    }

    public void Downgrade(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, EntitlementTier newTier, Timestamp at)
    {
        if (Status == EntitlementStatus.Revoked) throw EntitlementErrors.CannotDowngradeRevoked();
        if ((int)newTier >= (int)Tier) throw EntitlementErrors.InvalidScope();
        RaiseDomainEvent(new EntitlementDowngradedEvent(eventId, aggregateId, correlationId, causationId, EntitlementId, newTier, at));
    }

    public void Revoke(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string reason, Timestamp at)
    {
        Spec.EnsureActive(Status);
        RaiseDomainEvent(new EntitlementRevokedEvent(eventId, aggregateId, correlationId, causationId, EntitlementId, reason ?? string.Empty, at));
    }

    public void AttachScope(EntitlementScope scope)
    {
        Spec.EnsureActive(Status);
        if (!_scopes.ContainsKey(scope.Key))
            _scopes[scope.Key] = scope;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EntitlementGrantedEvent e:
                EntitlementId = e.EntitlementId;
                HolderRef = e.HolderRef;
                Tier = e.Tier;
                Status = EntitlementStatus.Granted;
                ValidFrom = e.ValidFrom;
                ValidUntil = e.ValidUntil;
                break;
            case EntitlementExtendedEvent e:
                Status = EntitlementStatus.Extended;
                ValidUntil = e.NewValidUntil;
                break;
            case EntitlementDowngradedEvent e:
                Status = EntitlementStatus.Downgraded;
                Tier = e.NewTier;
                break;
            case EntitlementRevokedEvent: Status = EntitlementStatus.Revoked; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(HolderRef))
            throw EntitlementErrors.HolderMissing();
    }
}
