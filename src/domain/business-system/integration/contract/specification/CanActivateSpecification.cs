namespace Whycespace.Domain.BusinessSystem.Integration.Contract;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ContractStatus status)
    {
        return status == ContractStatus.Draft;
    }
}
