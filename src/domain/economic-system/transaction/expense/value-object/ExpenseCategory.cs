namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public readonly record struct ExpenseCategory
{
    public string Value { get; }

    public ExpenseCategory(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ExpenseCategory cannot be empty.", nameof(value));
        Value = value.Trim();
    }

    public static ExpenseCategory From(string value) => new(value);

    public override string ToString() => Value;
}
