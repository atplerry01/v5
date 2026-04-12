namespace Whycespace.Domain.BusinessSystem.Integration.Endpoint;

public sealed class IsAvailableSpecification
{
    public bool IsSatisfiedBy(EndpointStatus status)
    {
        return status == EndpointStatus.Available;
    }
}
