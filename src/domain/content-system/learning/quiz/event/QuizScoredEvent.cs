using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Quiz;

public sealed record QuizScoredEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    QuizId QuizId, string LearnerRef, int Correct, int Total, Timestamp ScoredAt) : DomainEvent;
