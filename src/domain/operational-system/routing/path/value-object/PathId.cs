namespace Whycespace.Domain.OperationalSystem.Routing.Path;

public readonly record struct PathId
{
    public Guid Value { get; }

    public PathId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PathId cannot be empty.", nameof(value));

        Value = value;
    }

    public static PathId From(Guid value) => new(value);
}
