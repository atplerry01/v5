using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Module;

public sealed record ModuleCreatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    ModuleId ModuleId, string CourseRef, string Title, int Order, Timestamp CreatedAt) : DomainEvent;
