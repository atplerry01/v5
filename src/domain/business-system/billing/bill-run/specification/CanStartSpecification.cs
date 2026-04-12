namespace Whycespace.Domain.BusinessSystem.Billing.BillRun;

public sealed class CanStartSpecification
{
    public bool IsSatisfiedBy(BillRunStatus status)
    {
        return status == BillRunStatus.Created;
    }
}
