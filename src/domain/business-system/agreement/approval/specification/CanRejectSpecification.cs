namespace Whycespace.Domain.BusinessSystem.Agreement.Approval;

public sealed class CanRejectSpecification
{
    public bool IsSatisfiedBy(ApprovalStatus status)
    {
        return status == ApprovalStatus.Pending;
    }
}
