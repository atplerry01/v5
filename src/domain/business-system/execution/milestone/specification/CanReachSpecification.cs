namespace Whycespace.Domain.BusinessSystem.Execution.Milestone;

public sealed class CanReachSpecification
{
    public bool IsSatisfiedBy(MilestoneStatus status) => status == MilestoneStatus.Defined;
}
