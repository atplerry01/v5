namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public sealed class CanValidateSpecification
{
    public bool IsSatisfiedBy(TopologyStatus status)
    {
        return status == TopologyStatus.Defined;
    }
}

public sealed class CanLockSpecification
{
    public bool IsSatisfiedBy(TopologyStatus status)
    {
        return status == TopologyStatus.Validated;
    }
}
