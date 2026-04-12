namespace Whycespace.Domain.BusinessSystem.Integration.Client;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ClientStatus status)
    {
        return status == ClientStatus.Registered;
    }
}
