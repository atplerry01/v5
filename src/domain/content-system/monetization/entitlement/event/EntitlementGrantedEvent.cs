using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Entitlement;

public sealed record EntitlementGrantedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    EntitlementId EntitlementId, string HolderRef, EntitlementTier Tier,
    Timestamp ValidFrom, Timestamp ValidUntil, Timestamp GrantedAt) : DomainEvent;
