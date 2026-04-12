namespace Whycespace.Domain.BusinessSystem.Execution.Lifecycle;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(LifecycleStatus status)
    {
        return status == LifecycleStatus.Running;
    }
}
