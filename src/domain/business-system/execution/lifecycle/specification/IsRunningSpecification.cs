namespace Whycespace.Domain.BusinessSystem.Execution.Lifecycle;

public sealed class IsRunningSpecification
{
    public bool IsSatisfiedBy(LifecycleStatus status)
    {
        return status == LifecycleStatus.Running;
    }
}
