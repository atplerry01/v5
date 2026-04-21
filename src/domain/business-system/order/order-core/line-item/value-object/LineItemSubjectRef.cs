using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public enum LineItemSubjectKind
{
    Product,
    ServiceOffering,
    Bundle
}

public readonly record struct LineItemSubjectRef
{
    public LineItemSubjectKind Kind { get; }
    public LineItemSubjectId SubjectId { get; }

    public LineItemSubjectRef(LineItemSubjectKind kind, LineItemSubjectId subjectId)
    {
        Guard.Against(!Enum.IsDefined(kind), "LineItemSubjectKind is invalid.");
        Guard.Against(subjectId == default, "LineItemSubject id must not be empty.");

        Kind = kind;
        SubjectId = subjectId;
    }
}
