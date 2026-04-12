namespace Whycespace.Domain.BusinessSystem.Billing.BillRun;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(BillRunStatus status)
    {
        return status == BillRunStatus.Running;
    }
}
