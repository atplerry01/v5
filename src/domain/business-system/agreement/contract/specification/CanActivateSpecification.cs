namespace Whycespace.Domain.BusinessSystem.Agreement.Contract;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ContractStatus status)
    {
        return status == ContractStatus.Draft;
    }
}
