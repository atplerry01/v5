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
    public Guid SubjectId { get; }

    public LineItemSubjectRef(LineItemSubjectKind kind, Guid subjectId)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentException("LineItemSubjectKind is invalid.", nameof(kind));

        if (subjectId == Guid.Empty)
            throw new ArgumentException("LineItemSubject id must not be empty.", nameof(subjectId));

        Kind = kind;
        SubjectId = subjectId;
    }
}
