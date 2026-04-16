using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Certification;

public sealed class CertificationAggregate : AggregateRoot
{
    private static readonly CertificationSpecification Spec = new();

    public CertificationId CertificationId { get; private set; }
    public string CourseRef { get; private set; } = string.Empty;
    public string HolderRef { get; private set; } = string.Empty;
    public CertificateSerial Serial { get; private set; } = default!;
    public CertificationStatus Status { get; private set; }
    public Timestamp ValidFrom { get; private set; }
    public Timestamp ValidUntil { get; private set; }

    private CertificationAggregate() { }

    public static CertificationAggregate Issue(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        CertificationId id, string courseRef, string holderRef, CertificateSerial serial,
        Timestamp validFrom, Timestamp validUntil, Timestamp issuedAt)
    {
        if (string.IsNullOrWhiteSpace(courseRef)) throw CertificationErrors.InvalidCourseRef();
        if (string.IsNullOrWhiteSpace(holderRef)) throw CertificationErrors.InvalidHolderRef();
        Spec.EnsureValidityWindow(validFrom, validUntil);
        var agg = new CertificationAggregate();
        agg.RaiseDomainEvent(new CertificationIssuedEvent(
            eventId, aggregateId, correlationId, causationId, id, courseRef, holderRef, serial.Value,
            validFrom, validUntil, issuedAt));
        return agg;
    }

    public void Renew(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp newFrom, Timestamp newUntil, Timestamp at)
    {
        Spec.EnsureRenewable(Status);
        Spec.EnsureValidityWindow(newFrom, newUntil);
        RaiseDomainEvent(new CertificationRenewedEvent(eventId, aggregateId, correlationId, causationId, CertificationId, newFrom, newUntil, at));
    }

    public void Revoke(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string reason, Timestamp at)
    {
        if (Status == CertificationStatus.Revoked) throw CertificationErrors.AlreadyRevoked();
        RaiseDomainEvent(new CertificationRevokedEvent(eventId, aggregateId, correlationId, causationId, CertificationId, reason ?? string.Empty, at));
    }

    public void Expire(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == CertificationStatus.Expired) throw CertificationErrors.AlreadyExpired();
        if (Status == CertificationStatus.Revoked) throw CertificationErrors.AlreadyRevoked();
        RaiseDomainEvent(new CertificationExpiredEvent(eventId, aggregateId, correlationId, causationId, CertificationId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CertificationIssuedEvent e:
                CertificationId = e.CertificationId;
                CourseRef = e.CourseRef;
                HolderRef = e.HolderRef;
                Serial = CertificateSerial.Create(e.Serial);
                Status = CertificationStatus.Issued;
                ValidFrom = e.ValidFrom;
                ValidUntil = e.ValidUntil;
                break;
            case CertificationRenewedEvent e:
                Status = CertificationStatus.Renewed;
                ValidFrom = e.ValidFrom;
                ValidUntil = e.ValidUntil;
                break;
            case CertificationRevokedEvent: Status = CertificationStatus.Revoked; break;
            case CertificationExpiredEvent: Status = CertificationStatus.Expired; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(HolderRef))
            throw CertificationErrors.HolderMissing();
    }
}
