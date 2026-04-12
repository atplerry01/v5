namespace Whycespace.Domain.BusinessSystem.Agreement.Term;

public sealed class CanActivateTermSpecification
{
    public bool IsSatisfiedBy(TermStatus status)
    {
        return status == TermStatus.Draft;
    }
}
