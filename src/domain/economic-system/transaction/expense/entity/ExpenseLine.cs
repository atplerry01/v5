using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public sealed class ExpenseLine : Entity
{
    public Guid LineId { get; private set; }
    public decimal Amount { get; private set; }
    public ExpenseLineDescription Description { get; private set; } = ExpenseLineDescription.Empty;

    private ExpenseLine() { }

    internal static ExpenseLine Create(Guid lineId, decimal amount, ExpenseLineDescription description)
    {
        if (lineId == Guid.Empty)
            throw new ArgumentException("LineId cannot be empty.", nameof(lineId));
        if (amount <= 0m)
            throw ExpenseErrors.InvalidAmount();

        return new ExpenseLine
        {
            LineId = lineId,
            Amount = amount,
            Description = description
        };
    }

    // D-CONTENT-STR-EMBED-01 dual-path: legacy string overload normalizes to typed VO.
    internal static ExpenseLine Create(Guid lineId, decimal amount, string? description)
        => Create(lineId, amount, new ExpenseLineDescription(description));
}
