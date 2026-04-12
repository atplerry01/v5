namespace Whycespace.Domain.BusinessSystem.Execution.Stage;

public readonly record struct StageContextId
{
    public Guid Value { get; }
    public StageContextId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StageContextId value must not be empty.", nameof(value));
        Value = value;
    }
}
