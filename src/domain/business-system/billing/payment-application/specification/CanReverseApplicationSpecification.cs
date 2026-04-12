namespace Whycespace.Domain.BusinessSystem.Billing.PaymentApplication;

public sealed class CanReverseApplicationSpecification
{
    public bool IsSatisfiedBy(PaymentApplicationStatus status)
    {
        return status == PaymentApplicationStatus.Applied;
    }
}
