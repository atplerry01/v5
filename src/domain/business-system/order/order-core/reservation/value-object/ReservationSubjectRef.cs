namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public enum ReservationSubjectKind
{
    Product,
    ServiceOffering,
    Bundle
}

public readonly record struct ReservationSubjectRef
{
    public ReservationSubjectKind Kind { get; }
    public Guid SubjectId { get; }

    public ReservationSubjectRef(ReservationSubjectKind kind, Guid subjectId)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentException("ReservationSubjectKind is invalid.", nameof(kind));

        if (subjectId == Guid.Empty)
            throw new ArgumentException("ReservationSubject id must not be empty.", nameof(subjectId));

        Kind = kind;
        SubjectId = subjectId;
    }
}
