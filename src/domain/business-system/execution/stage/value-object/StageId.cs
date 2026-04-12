namespace Whycespace.Domain.BusinessSystem.Execution.Stage;

public readonly record struct StageId
{
    public Guid Value { get; }
    public StageId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StageId value must not be empty.", nameof(value));
        Value = value;
    }
}
