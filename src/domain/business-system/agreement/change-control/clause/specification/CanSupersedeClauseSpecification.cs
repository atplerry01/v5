namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;

public sealed class CanSupersedeClauseSpecification
{
    public bool IsSatisfiedBy(ClauseStatus status)
    {
        return status == ClauseStatus.Active;
    }
}
