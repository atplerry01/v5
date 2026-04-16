using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Lesson;

public sealed record LessonUpdatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    LessonId LessonId, string Body, Timestamp UpdatedAt) : DomainEvent;
