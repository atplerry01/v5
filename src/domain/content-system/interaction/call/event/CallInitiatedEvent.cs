using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Call;

public sealed record CallInitiatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CallId CallId, string InitiatorRef, CallMedium Medium, Timestamp InitiatedAt) : DomainEvent;
