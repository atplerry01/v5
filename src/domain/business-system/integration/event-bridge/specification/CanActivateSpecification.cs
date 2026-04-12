namespace Whycespace.Domain.BusinessSystem.Integration.EventBridge;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(EventBridgeStatus status)
    {
        return status is EventBridgeStatus.Defined or EventBridgeStatus.Disabled;
    }
}
