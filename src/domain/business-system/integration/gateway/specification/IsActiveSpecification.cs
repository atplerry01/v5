namespace Whycespace.Domain.BusinessSystem.Integration.Gateway;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(GatewayStatus status)
    {
        return status == GatewayStatus.Active;
    }
}
