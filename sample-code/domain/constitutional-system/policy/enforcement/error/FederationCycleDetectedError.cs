using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class FederationCycleDetectedError : DomainException
{
    public FederationCycleDetectedError(string message)
        : base("FEDERATION_CYCLE_DETECTED", message) { }
}
