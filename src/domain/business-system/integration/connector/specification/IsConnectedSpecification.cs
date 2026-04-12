namespace Whycespace.Domain.BusinessSystem.Integration.Connector;

public sealed class IsConnectedSpecification
{
    public bool IsSatisfiedBy(ConnectorStatus status)
    {
        return status == ConnectorStatus.Connected;
    }
}
