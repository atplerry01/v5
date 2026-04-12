namespace Whycespace.Domain.BusinessSystem.Integration.CommandBridge;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(CommandBridgeStatus status)
    {
        return status == CommandBridgeStatus.Active;
    }
}
