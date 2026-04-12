namespace Whycespace.Domain.BusinessSystem.Integration.Schema;

public readonly record struct SchemaDefinitionId
{
    public Guid Value { get; }

    public SchemaDefinitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SchemaDefinitionId value must not be empty.", nameof(value));

        Value = value;
    }
}
