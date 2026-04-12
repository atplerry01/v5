namespace Whycespace.Domain.BusinessSystem.Billing.PaymentApplication;

public sealed class CanApplyPaymentSpecification
{
    public bool IsSatisfiedBy(PaymentApplicationStatus status)
    {
        return status == PaymentApplicationStatus.Pending;
    }
}
