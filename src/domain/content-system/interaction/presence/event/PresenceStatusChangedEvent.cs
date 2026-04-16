using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Presence;

public sealed record PresenceStatusChangedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    PresenceId PresenceId, PresenceStatus Status, Timestamp ChangedAt) : DomainEvent;
