using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public sealed class ExpenseLifecycleSpecification : Specification<(ExpenseStatus From, ExpenseStatus To)>
{
    public override bool IsSatisfiedBy((ExpenseStatus From, ExpenseStatus To) transition) =>
        transition switch
        {
            (ExpenseStatus.Created, ExpenseStatus.Recorded) => true,
            (ExpenseStatus.Created, ExpenseStatus.Cancelled) => true,
            _ => false
        };
}
