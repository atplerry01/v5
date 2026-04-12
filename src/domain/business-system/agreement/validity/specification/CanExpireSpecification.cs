namespace Whycespace.Domain.BusinessSystem.Agreement.Validity;

public sealed class CanExpireSpecification
{
    public bool IsSatisfiedBy(ValidityStatus status)
    {
        return status == ValidityStatus.Valid;
    }
}
