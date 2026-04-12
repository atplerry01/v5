namespace Whycespace.Domain.BusinessSystem.Execution.Sourcing;

public sealed class CanFailSpecification
{
    public bool IsSatisfiedBy(SourcingStatus status) => status == SourcingStatus.Requested;
}
