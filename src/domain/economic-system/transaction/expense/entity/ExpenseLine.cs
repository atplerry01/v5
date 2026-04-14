using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public sealed class ExpenseLine : Entity
{
    public Guid LineId { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private ExpenseLine() { }

    internal static ExpenseLine Create(Guid lineId, decimal amount, string description)
    {
        if (lineId == Guid.Empty)
            throw new ArgumentException("LineId cannot be empty.", nameof(lineId));
        if (amount <= 0m)
            throw ExpenseErrors.InvalidAmount();

        return new ExpenseLine
        {
            LineId = lineId,
            Amount = amount,
            Description = description ?? string.Empty
        };
    }
}
