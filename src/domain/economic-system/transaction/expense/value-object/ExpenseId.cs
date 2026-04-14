namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public readonly record struct ExpenseId
{
    public Guid Value { get; }

    public ExpenseId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ExpenseId cannot be empty.", nameof(value));
        Value = value;
    }

    public static ExpenseId From(Guid value) => new(value);
}
