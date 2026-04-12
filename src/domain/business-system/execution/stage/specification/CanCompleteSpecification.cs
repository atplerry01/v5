namespace Whycespace.Domain.BusinessSystem.Execution.Stage;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(StageStatus status) => status == StageStatus.InProgress;
}
