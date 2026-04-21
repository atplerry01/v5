using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.BudgetControl;

public readonly record struct BudgetControlId
{
    public Guid Value { get; }

    public BudgetControlId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "BudgetControlId cannot be empty.");
        Value = value;
    }
}
