using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Presence;

public sealed record PresenceRegisteredEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    PresenceId PresenceId, string ActorRef, PresenceStatus InitialStatus, Timestamp RegisteredAt) : DomainEvent;
