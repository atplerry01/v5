namespace Whycespace.Domain.BusinessSystem.Entitlement.Right;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(RightStatus status)
    {
        return status == RightStatus.Active;
    }
}
