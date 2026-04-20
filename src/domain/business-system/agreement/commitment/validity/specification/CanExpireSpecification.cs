namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Validity;

public sealed class CanExpireSpecification
{
    public bool IsSatisfiedBy(ValidityStatus status)
    {
        return status == ValidityStatus.Valid;
    }
}
