namespace Whycespace.Domain.BusinessSystem.Integration.Mapping;

public readonly record struct MappingId
{
    public Guid Value { get; }

    public MappingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MappingId value must not be empty.", nameof(value));
        Value = value;
    }
}
