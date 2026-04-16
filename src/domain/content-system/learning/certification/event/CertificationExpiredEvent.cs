using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Certification;

public sealed record CertificationExpiredEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CertificationId CertificationId, Timestamp ExpiredAt) : DomainEvent;
