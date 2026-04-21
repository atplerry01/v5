using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

/// <summary>
/// D-CONTENT-STR-EMBED-01 — typed structural-descriptive VO replacing the raw
/// <c>string Description</c> previously embedded on <see cref="ExpenseLine"/>.
/// Allows empty (line descriptions are optional).
/// </summary>
public readonly record struct ExpenseLineDescription
{
    public string Value { get; }

    public ExpenseLineDescription(string? value)
    {
        Value = value ?? string.Empty;
    }

    public static ExpenseLineDescription Empty => new(string.Empty);
}
