using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Lesson;

public sealed record LessonPublishedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    LessonId LessonId, Timestamp PublishedAt) : DomainEvent;
