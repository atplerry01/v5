namespace Whycespace.Domain.BusinessSystem.Integration.Contract;

public readonly record struct ContractSchemaId
{
    public Guid Value { get; }

    public ContractSchemaId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ContractSchemaId value must not be empty.", nameof(value));
        Value = value;
    }
}
