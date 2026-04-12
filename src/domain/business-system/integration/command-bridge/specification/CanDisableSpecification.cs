namespace Whycespace.Domain.BusinessSystem.Integration.CommandBridge;

public sealed class CanDisableSpecification
{
    public bool IsSatisfiedBy(CommandBridgeStatus status)
    {
        return status == CommandBridgeStatus.Active;
    }
}
