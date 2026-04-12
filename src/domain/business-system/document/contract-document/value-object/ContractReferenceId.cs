namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public readonly record struct ContractReferenceId
{
    public Guid Value { get; }

    public ContractReferenceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ContractReferenceId value must not be empty.", nameof(value));
        Value = value;
    }
}
