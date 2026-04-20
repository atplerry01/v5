namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ContractStatus status)
    {
        return status == ContractStatus.Draft;
    }
}
