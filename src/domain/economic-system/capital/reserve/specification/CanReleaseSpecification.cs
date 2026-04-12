using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public sealed class CanReleaseSpecification : Specification<ReserveAggregate>
{
    public override bool IsSatisfiedBy(ReserveAggregate reserve) =>
        reserve.Status == ReserveStatus.Active;
}
