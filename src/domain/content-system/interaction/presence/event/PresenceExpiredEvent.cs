using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Presence;

public sealed record PresenceExpiredEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    PresenceId PresenceId, Timestamp ExpiredAt) : DomainEvent;
