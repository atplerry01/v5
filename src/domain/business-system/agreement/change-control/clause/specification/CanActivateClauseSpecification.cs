namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;

public sealed class CanActivateClauseSpecification
{
    public bool IsSatisfiedBy(ClauseStatus status)
    {
        return status == ClauseStatus.Draft;
    }
}
