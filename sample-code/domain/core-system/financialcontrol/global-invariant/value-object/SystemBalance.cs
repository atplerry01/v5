namespace Whycespace.Domain.CoreSystem.FinancialControl.GlobalInvariant;

/// <summary>
/// Represents the global system balance. Invariant: must never be negative.
/// </summary>
public sealed record SystemBalance
{
    public decimal Value { get; }
    public bool IsNegative => Value < 0m;
    public bool IsZero => Value == 0m;
    public bool IsPositive => Value > 0m;

    private SystemBalance(decimal value) => Value = value;

    public static SystemBalance Initial() => new(0m);

    public static SystemBalance From(decimal value) => new(value);

    public SystemBalance Credit(decimal amount) =>
        amount <= 0m
            ? throw new ArgumentOutOfRangeException(nameof(amount), "Credit amount must be positive.")
            : new(Value + amount);

    public SystemBalance Debit(decimal amount) =>
        amount <= 0m
            ? throw new ArgumentOutOfRangeException(nameof(amount), "Debit amount must be positive.")
            : new(Value - amount);
}
