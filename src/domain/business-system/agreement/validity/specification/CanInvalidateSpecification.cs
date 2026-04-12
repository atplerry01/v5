namespace Whycespace.Domain.BusinessSystem.Agreement.Validity;

public sealed class CanInvalidateSpecification
{
    public bool IsSatisfiedBy(ValidityStatus status)
    {
        return status == ValidityStatus.Valid;
    }
}
