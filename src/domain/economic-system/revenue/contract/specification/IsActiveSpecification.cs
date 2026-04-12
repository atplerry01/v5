using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Contract;

public sealed class IsActiveSpecification : Specification<RevenueContractAggregate>
{
    public override bool IsSatisfiedBy(RevenueContractAggregate contract) =>
        contract.Status == ContractStatus.Active;
}
