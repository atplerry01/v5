namespace Whycespace.Domain.BusinessSystem.Integration.Client;

public readonly record struct ExternalClientId
{
    public Guid Value { get; }

    public ExternalClientId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ExternalClientId value must not be empty.", nameof(value));
        Value = value;
    }
}
