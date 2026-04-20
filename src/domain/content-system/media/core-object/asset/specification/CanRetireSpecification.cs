using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public sealed class CanRetireSpecification : Specification<AssetAggregate>
{
    public override bool IsSatisfiedBy(AssetAggregate entity)
        => entity.Status != AssetStatus.Retired;
}
