namespace Whycespace.Domain.BusinessSystem.Integration.Endpoint;

public sealed class CanMarkUnavailableSpecification
{
    public bool IsSatisfiedBy(EndpointStatus status)
    {
        return status == EndpointStatus.Available;
    }
}
