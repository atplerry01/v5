namespace Whycespace.Domain.OperationalSystem.Deployment.Activation;

public sealed class RegionActivationService
{
    public bool IsOperational(RegionActivationAggregate region) => region.Status.IsOperational;
    public bool IsHalted(RegionActivationAggregate region) => region.Status == ActivationStatus.Halted;
    public bool CanPromoteToFull(RegionActivationAggregate region) => region.Status == ActivationStatus.Canary;
}
