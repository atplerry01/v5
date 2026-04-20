namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;

public sealed class CanTerminateSpecification
{
    public bool IsSatisfiedBy(ContractStatus status)
    {
        return status is ContractStatus.Active or ContractStatus.Suspended;
    }
}
