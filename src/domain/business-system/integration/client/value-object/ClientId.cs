namespace Whycespace.Domain.BusinessSystem.Integration.Client;

public readonly record struct ClientId
{
    public Guid Value { get; }

    public ClientId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ClientId value must not be empty.", nameof(value));
        Value = value;
    }
}
