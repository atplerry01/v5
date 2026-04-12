namespace Whycespace.Domain.BusinessSystem.Agreement.Term;

public sealed class CanExpireTermSpecification
{
    public bool IsSatisfiedBy(TermStatus status)
    {
        return status == TermStatus.Active;
    }
}
