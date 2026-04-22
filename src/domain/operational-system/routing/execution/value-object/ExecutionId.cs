namespace Whycespace.Domain.OperationalSystem.Routing.Execution;

public readonly record struct ExecutionId
{
    public Guid Value { get; }

    public ExecutionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ExecutionId cannot be empty.", nameof(value));

        Value = value;
    }

    public static ExecutionId From(Guid value) => new(value);
}
