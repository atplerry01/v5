namespace Whycespace.Domain.BusinessSystem.Integration.Connector;

public readonly record struct ConnectorTargetId
{
    public Guid Value { get; }

    public ConnectorTargetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ConnectorTargetId value must not be empty.", nameof(value));
        Value = value;
    }
}
