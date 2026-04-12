using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Asset;

public sealed class CanRevalueSpecification : Specification<AssetAggregate>
{
    public override bool IsSatisfiedBy(AssetAggregate entity)
        => entity.Status != AssetStatus.Disposed;
}
