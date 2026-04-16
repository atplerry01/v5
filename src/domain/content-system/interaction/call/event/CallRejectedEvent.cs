using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Call;

public sealed record CallRejectedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CallId CallId, string ActorRef, Timestamp RejectedAt) : DomainEvent;
