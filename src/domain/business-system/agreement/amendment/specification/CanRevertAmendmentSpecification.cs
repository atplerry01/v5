namespace Whycespace.Domain.BusinessSystem.Agreement.Amendment;

public sealed class CanRevertAmendmentSpecification
{
    public bool IsSatisfiedBy(AmendmentStatus status)
    {
        return status == AmendmentStatus.Applied;
    }
}
