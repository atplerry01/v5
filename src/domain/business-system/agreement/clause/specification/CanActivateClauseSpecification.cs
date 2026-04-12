namespace Whycespace.Domain.BusinessSystem.Agreement.Clause;

public sealed class CanActivateClauseSpecification
{
    public bool IsSatisfiedBy(ClauseStatus status)
    {
        return status == ClauseStatus.Draft;
    }
}
