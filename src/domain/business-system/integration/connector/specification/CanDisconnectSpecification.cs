namespace Whycespace.Domain.BusinessSystem.Integration.Connector;

public sealed class CanDisconnectSpecification
{
    public bool IsSatisfiedBy(ConnectorStatus status)
    {
        return status == ConnectorStatus.Connected;
    }
}
