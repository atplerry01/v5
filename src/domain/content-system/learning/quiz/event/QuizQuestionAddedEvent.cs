using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Quiz;

public sealed record QuizQuestionAddedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    QuizId QuizId, Guid QuestionId, string Prompt, int Points, Timestamp AddedAt) : DomainEvent;
