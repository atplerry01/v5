namespace Whycespace.Domain.BusinessSystem.Agreement.Amendment;

public sealed class CanApplyAmendmentSpecification
{
    public bool IsSatisfiedBy(AmendmentStatus status)
    {
        return status == AmendmentStatus.Draft;
    }
}
