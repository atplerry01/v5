using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Entitlement;

public sealed record EntitlementRevokedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    EntitlementId EntitlementId, string Reason, Timestamp RevokedAt) : DomainEvent;
