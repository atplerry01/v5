namespace Whycespace.Domain.BusinessSystem.Integration.CommandBridge;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(CommandBridgeStatus status)
    {
        return status is CommandBridgeStatus.Defined or CommandBridgeStatus.Disabled;
    }
}
