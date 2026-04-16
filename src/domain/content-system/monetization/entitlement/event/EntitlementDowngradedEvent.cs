using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Entitlement;

public sealed record EntitlementDowngradedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    EntitlementId EntitlementId, EntitlementTier NewTier, Timestamp DowngradedAt) : DomainEvent;
