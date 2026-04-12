using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;

public readonly record struct DiscrepancyId
{
    public Guid Value { get; }

    public DiscrepancyId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DiscrepancyId cannot be empty.");
        Value = value;
    }
}
