namespace Whycespace.Domain.BusinessSystem.Integration.Callback;

public sealed class CanMarkPendingSpecification
{
    public bool IsSatisfiedBy(CallbackStatus status)
    {
        return status == CallbackStatus.Registered;
    }
}
