namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

public readonly record struct CounterpartyId
{
    public Guid Value { get; }

    public CounterpartyId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CounterpartyId value must not be empty.", nameof(value));

        Value = value;
    }
}
