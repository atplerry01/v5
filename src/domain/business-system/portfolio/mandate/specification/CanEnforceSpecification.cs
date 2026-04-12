namespace Whycespace.Domain.BusinessSystem.Portfolio.Mandate;

public sealed class CanEnforceSpecification
{
    public bool IsSatisfiedBy(MandateStatus status)
    {
        return status == MandateStatus.Draft;
    }
}
