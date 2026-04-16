using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed class AlreadyOpenSpecification : Specification<CapitalAccountAggregate>
{
    public override bool IsSatisfiedBy(CapitalAccountAggregate entity) =>
        entity.Version >= 0;
}
