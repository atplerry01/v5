namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public sealed class CanRevertAmendmentSpecification
{
    public bool IsSatisfiedBy(AmendmentStatus status)
    {
        return status == AmendmentStatus.Applied;
    }
}
