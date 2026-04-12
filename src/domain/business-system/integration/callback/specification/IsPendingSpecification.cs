namespace Whycespace.Domain.BusinessSystem.Integration.Callback;

public sealed class IsPendingSpecification
{
    public bool IsSatisfiedBy(CallbackStatus status)
    {
        return status == CallbackStatus.Pending;
    }
}
