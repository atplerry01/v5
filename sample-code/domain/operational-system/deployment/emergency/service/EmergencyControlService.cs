namespace Whycespace.Domain.OperationalSystem.Deployment.Emergency;

public sealed class EmergencyControlService
{
    public bool IsHaltActive(EmergencyControlAggregate control) => control.Status == EmergencyStatus.Active;
    public bool CanResolve(EmergencyControlAggregate control) => control.Status == EmergencyStatus.Active;
}
