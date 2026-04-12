using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed class CanReleaseSpecification : Specification<BindingAggregate>
{
    public override bool IsSatisfiedBy(BindingAggregate entity)
        => entity.Status == BindingStatus.Active;
}
