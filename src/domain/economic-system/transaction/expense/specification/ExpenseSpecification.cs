using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public sealed class ExpenseSpecification : Specification<ExpenseAggregate>
{
    public override bool IsSatisfiedBy(ExpenseAggregate expense)
    {
        if (expense is null) return false;
        if (expense.Amount.Value <= 0m) return false;
        if (string.IsNullOrWhiteSpace(expense.SourceReference.Value)) return false;
        return true;
    }
}
