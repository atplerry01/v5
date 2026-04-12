namespace Whycespace.Domain.BusinessSystem.Billing.BillRun;

public sealed class CanFailSpecification
{
    public bool IsSatisfiedBy(BillRunStatus status)
    {
        return status == BillRunStatus.Running;
    }
}
