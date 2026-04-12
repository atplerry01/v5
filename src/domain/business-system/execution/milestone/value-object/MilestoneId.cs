namespace Whycespace.Domain.BusinessSystem.Execution.Milestone;

public readonly record struct MilestoneId
{
    public Guid Value { get; }
    public MilestoneId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MilestoneId value must not be empty.", nameof(value));
        Value = value;
    }
}
