namespace Whycespace.Domain.BusinessSystem.Integration.EventBridge;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(EventBridgeStatus status)
    {
        return status == EventBridgeStatus.Active;
    }
}
