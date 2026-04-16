using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Quiz;

public sealed record QuizPublishedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    QuizId QuizId, Timestamp PublishedAt) : DomainEvent;
