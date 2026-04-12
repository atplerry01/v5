namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public sealed class CanReleaseSpecification
{
    public bool IsSatisfiedBy(DispatchStatus status)
    {
        return status == DispatchStatus.Created;
    }
}
