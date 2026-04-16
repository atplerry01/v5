using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Course;

public sealed record CourseDraftedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CourseId CourseId, string OwnerRef, string Title, Timestamp DraftedAt) : DomainEvent;
