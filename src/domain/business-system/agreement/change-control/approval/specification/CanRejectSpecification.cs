namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Approval;

public sealed class CanRejectSpecification
{
    public bool IsSatisfiedBy(ApprovalStatus status)
    {
        return status == ApprovalStatus.Pending;
    }
}
