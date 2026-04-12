using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public sealed class CanDeactivateSpecification : Specification<FxAggregate>
{
    public override bool IsSatisfiedBy(FxAggregate fx)
    {
        return fx.Status == FxStatus.Active;
    }
}
