using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

public sealed class SettlementSpecification : Specification<SettlementAggregate>
{
    public override bool IsSatisfiedBy(SettlementAggregate settlement)
    {
        if (settlement is null) return false;
        if (settlement.Amount.Value <= 0m) return false;
        if (string.IsNullOrWhiteSpace(settlement.SourceReference.Value)) return false;
        if (string.IsNullOrWhiteSpace(settlement.Provider.Value)) return false;
        return true;
    }
}
