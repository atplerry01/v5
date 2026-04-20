namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public sealed class CanApplyAmendmentSpecification
{
    public bool IsSatisfiedBy(AmendmentStatus status)
    {
        return status == AmendmentStatus.Draft;
    }
}
