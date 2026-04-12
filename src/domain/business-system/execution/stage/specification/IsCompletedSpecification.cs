namespace Whycespace.Domain.BusinessSystem.Execution.Stage;

public sealed class IsCompletedSpecification
{
    public bool IsSatisfiedBy(StageStatus status) => status == StageStatus.Completed;
}
