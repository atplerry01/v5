using Whycespace.Domain.SharedKernel.Primitives.Kernel;

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
    public ReservationSubjectId SubjectId { get; }

    public ReservationSubjectRef(ReservationSubjectKind kind, ReservationSubjectId subjectId)
    {
        Guard.Against(!Enum.IsDefined(kind), "ReservationSubjectKind is invalid.");
        Guard.Against(subjectId == default, "ReservationSubject id must not be empty.");

        Kind = kind;
        SubjectId = subjectId;
    }
}
