namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;

public readonly record struct ContractId
{
    public Guid Value { get; }

    public ContractId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ContractId value must not be empty.", nameof(value));

        Value = value;
    }
}
