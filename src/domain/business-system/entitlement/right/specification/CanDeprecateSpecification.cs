namespace Whycespace.Domain.BusinessSystem.Entitlement.Right;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(RightStatus status)
    {
        return status == RightStatus.Active;
    }
}
