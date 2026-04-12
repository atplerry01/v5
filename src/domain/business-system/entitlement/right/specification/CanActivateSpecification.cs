namespace Whycespace.Domain.BusinessSystem.Entitlement.Right;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(RightStatus status)
    {
        return status == RightStatus.Defined;
    }
}
