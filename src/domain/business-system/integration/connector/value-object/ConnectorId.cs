namespace Whycespace.Domain.BusinessSystem.Integration.Connector;

public readonly record struct ConnectorId
{
    public Guid Value { get; }

    public ConnectorId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ConnectorId value must not be empty.", nameof(value));
        Value = value;
    }
}
