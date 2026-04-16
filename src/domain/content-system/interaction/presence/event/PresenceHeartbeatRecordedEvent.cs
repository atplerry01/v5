using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Presence;

public sealed record PresenceHeartbeatRecordedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    PresenceId PresenceId, Timestamp HeartbeatAt) : DomainEvent;
