using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Certification;

public sealed record CertificationRenewedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CertificationId CertificationId, Timestamp ValidFrom, Timestamp ValidUntil, Timestamp RenewedAt) : DomainEvent;
