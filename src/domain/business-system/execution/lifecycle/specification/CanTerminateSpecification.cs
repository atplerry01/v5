namespace Whycespace.Domain.BusinessSystem.Execution.Lifecycle;

public sealed class CanTerminateSpecification
{
    public bool IsSatisfiedBy(LifecycleStatus status)
    {
        return status == LifecycleStatus.Running;
    }
}
