namespace Whycespace.Domain.BusinessSystem.Integration.EventBridge;

public sealed class CanDisableSpecification
{
    public bool IsSatisfiedBy(EventBridgeStatus status)
    {
        return status == EventBridgeStatus.Active;
    }
}
