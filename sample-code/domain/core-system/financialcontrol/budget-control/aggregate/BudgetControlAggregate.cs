using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.BudgetControl;

public sealed class BudgetControlAggregate : AggregateRoot
{
    public string BudgetName { get; private set; } = string.Empty;
    public decimal AllocatedAmount { get; private set; }

    public static BudgetControlAggregate Create(Guid id, string budgetName, decimal allocatedAmount)
    {
        var agg = new BudgetControlAggregate
        {
            Id = id,
            BudgetName = budgetName,
            AllocatedAmount = allocatedAmount
        };
        agg.RaiseDomainEvent(new BudgetControlCreatedEvent(id, budgetName, allocatedAmount));
        return agg;
    }

    public void Adjust(decimal newAmount)
    {
        AllocatedAmount = newAmount;
        RaiseDomainEvent(new BudgetControlAdjustedEvent(Id, newAmount));
    }
}
