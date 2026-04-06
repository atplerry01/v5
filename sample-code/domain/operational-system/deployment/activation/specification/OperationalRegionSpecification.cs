namespace Whycespace.Domain.OperationalSystem.Deployment.Activation;

public sealed class OperationalRegionSpecification
{
    public bool IsSatisfiedBy(RegionActivationAggregate region) => region.Status.IsOperational;
}
