namespace Whycespace.Domain.BusinessSystem.Execution.Milestone;

public sealed class IsReachedSpecification
{
    public bool IsSatisfiedBy(MilestoneStatus status) => status == MilestoneStatus.Reached;
}
