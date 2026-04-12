namespace Whycespace.Domain.BusinessSystem.Integration.Client;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(ClientStatus status)
    {
        return status == ClientStatus.Active;
    }
}
