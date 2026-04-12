namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public readonly record struct SpvId
{
    public Guid Value { get; }

    public SpvId(Guid value)
    {
        if (value == Guid.Empty)
            throw SpvErrors.MissingId();

        Value = value;
    }
}
