namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed record Balance
{
    public Amount Value { get; }
    public Currency Currency { get; }

    public Balance(Amount value, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        Value = value;
        Currency = currency;
    }

    public static Balance Zero(Currency currency) => new(Amount.Zero, currency);

    public Balance Credit(Amount amount) => new(Value + amount, Currency);

    public Balance Debit(Amount amount) => new(Value - amount, Currency);

    public override string ToString() => $"{Value} {Currency.Code}";
}
