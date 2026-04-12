namespace Whycespace.Domain.BusinessSystem.Execution.Milestone;

public sealed class CanMissSpecification
{
    public bool IsSatisfiedBy(MilestoneStatus status) => status == MilestoneStatus.Defined;
}
