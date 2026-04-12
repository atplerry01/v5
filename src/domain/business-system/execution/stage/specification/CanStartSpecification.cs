namespace Whycespace.Domain.BusinessSystem.Execution.Stage;

public sealed class CanStartSpecification
{
    public bool IsSatisfiedBy(StageStatus status) => status == StageStatus.Initialized;
}
