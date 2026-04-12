namespace Whycespace.Domain.BusinessSystem.Integration.Adapter;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(AdapterStatus status)
    {
        return status == AdapterStatus.Defined;
    }
}
