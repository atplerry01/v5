namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public sealed class ExpenseService
{
    public decimal CalculateTotal(IReadOnlyList<ExpenseAggregate> expenses)
    {
        if (expenses is null || expenses.Count == 0) return 0m;

        decimal total = 0m;
        foreach (var expense in expenses)
        {
            if (expense.Status == ExpenseStatus.Cancelled) continue;
            total += expense.Amount.Value;
        }
        return total;
    }
}
