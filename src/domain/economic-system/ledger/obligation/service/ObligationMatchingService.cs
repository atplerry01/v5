using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed class ObligationMatchingService
{
    public bool ValidateSettlementMatch(ObligationAggregate obligation, Amount settlementAmount) =>
        settlementAmount.Value >= obligation.Amount.Value;
}
