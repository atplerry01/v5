namespace Whycespace.Domain.BusinessSystem.Integration.Adapter;

public readonly record struct AdapterTypeId
{
    public Guid Value { get; }

    public AdapterTypeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AdapterTypeId value must not be empty.", nameof(value));
        Value = value;
    }
}
