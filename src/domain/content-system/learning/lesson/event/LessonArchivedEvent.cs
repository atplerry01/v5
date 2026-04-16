using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Lesson;

public sealed record LessonArchivedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    LessonId LessonId, Timestamp ArchivedAt) : DomainEvent;
