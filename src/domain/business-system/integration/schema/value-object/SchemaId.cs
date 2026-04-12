namespace Whycespace.Domain.BusinessSystem.Integration.Schema;

public readonly record struct SchemaId
{
    public Guid Value { get; }

    public SchemaId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SchemaId value must not be empty.", nameof(value));

        Value = value;
    }
}
