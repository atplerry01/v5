using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Entitlement;

public sealed record EntitlementExtendedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    EntitlementId EntitlementId, Timestamp NewValidUntil, Timestamp ExtendedAt) : DomainEvent;
