using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Course;

public sealed record CourseModuleAttachedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CourseId CourseId, string ModuleRef, int Order, Timestamp AttachedAt) : DomainEvent;
