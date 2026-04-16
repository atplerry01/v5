using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Certification;

public sealed record CertificationIssuedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CertificationId CertificationId, string CourseRef, string HolderRef, string Serial,
    Timestamp ValidFrom, Timestamp ValidUntil, Timestamp IssuedAt) : DomainEvent;
