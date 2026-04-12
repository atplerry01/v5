namespace Whycespace.Domain.StructuralSystem.Structure.Classification;

public readonly record struct ClassificationId
{
    public Guid Value { get; }

    public ClassificationId(Guid value)
    {
        if (value == Guid.Empty)
            throw ClassificationErrors.MissingId();

        Value = value;
    }
}
