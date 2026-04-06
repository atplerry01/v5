using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Activation;

public sealed class RegionHaltedException : DomainException
{
    public RegionHaltedException(string regionId)
        : base("REGION_HALTED", $"Region '{regionId}' is halted. Operations are suspended.") { }
}

public sealed class RegionNotOperationalException : DomainException
{
    public RegionNotOperationalException(string regionId, string status)
        : base("REGION_NOT_OPERATIONAL", $"Region '{regionId}' is not operational. Current status: {status}.") { }
}
