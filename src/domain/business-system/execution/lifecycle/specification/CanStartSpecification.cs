namespace Whycespace.Domain.BusinessSystem.Execution.Lifecycle;

public sealed class CanStartSpecification
{
    public bool IsSatisfiedBy(LifecycleStatus status)
    {
        return status == LifecycleStatus.Initialized;
    }
}
