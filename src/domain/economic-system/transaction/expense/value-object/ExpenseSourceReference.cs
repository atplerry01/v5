namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public readonly record struct ExpenseSourceReference
{
    public string Value { get; }

    public ExpenseSourceReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ExpenseSourceReference cannot be empty.", nameof(value));
        Value = value.Trim();
    }

    public static ExpenseSourceReference From(string value) => new(value);

    public override string ToString() => Value;
}
