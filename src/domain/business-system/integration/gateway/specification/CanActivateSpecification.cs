namespace Whycespace.Domain.BusinessSystem.Integration.Gateway;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(GatewayStatus status)
    {
        return status == GatewayStatus.Defined;
    }
}
