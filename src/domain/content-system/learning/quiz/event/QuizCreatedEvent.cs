using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Quiz;

public sealed record QuizCreatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    QuizId QuizId, string CourseRef, Timestamp CreatedAt) : DomainEvent;
