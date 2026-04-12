namespace Whycespace.Domain.BusinessSystem.Integration.Client;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(ClientStatus status)
    {
        return status == ClientStatus.Active;
    }
}
