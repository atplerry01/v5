using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Module;

public sealed record ModuleReorderedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ModuleId ModuleId, int Order, Timestamp ReorderedAt) : DomainEvent;
