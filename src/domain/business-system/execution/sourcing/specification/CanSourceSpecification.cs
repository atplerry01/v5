namespace Whycespace.Domain.BusinessSystem.Execution.Sourcing;

public sealed class CanSourceSpecification
{
    public bool IsSatisfiedBy(SourcingStatus status) => status == SourcingStatus.Requested;
}
