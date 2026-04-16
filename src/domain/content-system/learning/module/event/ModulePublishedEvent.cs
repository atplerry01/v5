using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Module;

public sealed record ModulePublishedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ModuleId ModuleId, Timestamp PublishedAt) : DomainEvent;
