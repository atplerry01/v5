namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public readonly record struct ContractDocumentId
{
    public Guid Value { get; }

    public ContractDocumentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ContractDocumentId value must not be empty.", nameof(value));
        Value = value;
    }
}
