namespace Whycespace.Domain.BusinessSystem.Execution.Sourcing;

public sealed class IsSourcedSpecification
{
    public bool IsSatisfiedBy(SourcingStatus status) => status == SourcingStatus.Sourced;
}
