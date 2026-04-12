namespace Whycespace.Domain.BusinessSystem.Execution.Milestone;

public readonly record struct MilestoneTargetId
{
    public Guid Value { get; }

    public MilestoneTargetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MilestoneTargetId value must not be empty.", nameof(value));
        Value = value;
    }
}
