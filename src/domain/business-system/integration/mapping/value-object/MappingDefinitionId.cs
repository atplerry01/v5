namespace Whycespace.Domain.BusinessSystem.Integration.Mapping;

public readonly record struct MappingDefinitionId
{
    public Guid Value { get; }

    public MappingDefinitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MappingDefinitionId value must not be empty.", nameof(value));
        Value = value;
    }
}
