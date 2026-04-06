using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Emergency;

public sealed class EmergencyHaltActiveException : DomainException
{
    public EmergencyHaltActiveException(string scope, string targetId)
        : base("EMERGENCY_HALT_ACTIVE", $"Emergency halt is active for {scope}:{targetId}. All operations suspended.") { }
}
