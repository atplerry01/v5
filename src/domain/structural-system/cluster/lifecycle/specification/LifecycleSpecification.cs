namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed class CanTransitionSpecification
{
    public bool IsSatisfiedBy(LifecycleStatus status)
    {
        return status == LifecycleStatus.Defined;
    }
}

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(LifecycleStatus status)
    {
        return status == LifecycleStatus.Transitioned;
    }
}
