using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Lesson;

public sealed record LessonCreatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    LessonId LessonId, string ModuleRef, string Body, Timestamp CreatedAt) : DomainEvent;
