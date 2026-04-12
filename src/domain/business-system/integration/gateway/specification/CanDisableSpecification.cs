namespace Whycespace.Domain.BusinessSystem.Integration.Gateway;

public sealed class CanDisableSpecification
{
    public bool IsSatisfiedBy(GatewayStatus status)
    {
        return status == GatewayStatus.Active;
    }
}
