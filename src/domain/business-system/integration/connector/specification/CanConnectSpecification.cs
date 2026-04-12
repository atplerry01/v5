namespace Whycespace.Domain.BusinessSystem.Integration.Connector;

public sealed class CanConnectSpecification
{
    public bool IsSatisfiedBy(ConnectorStatus status)
    {
        return status == ConnectorStatus.Defined;
    }
}
