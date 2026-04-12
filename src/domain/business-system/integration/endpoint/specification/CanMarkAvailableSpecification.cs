namespace Whycespace.Domain.BusinessSystem.Integration.Endpoint;

public sealed class CanMarkAvailableSpecification
{
    public bool IsSatisfiedBy(EndpointStatus status)
    {
        return status == EndpointStatus.Defined || status == EndpointStatus.Unavailable;
    }
}
