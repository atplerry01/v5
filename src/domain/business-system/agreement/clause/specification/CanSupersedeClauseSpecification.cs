namespace Whycespace.Domain.BusinessSystem.Agreement.Clause;

public sealed class CanSupersedeClauseSpecification
{
    public bool IsSatisfiedBy(ClauseStatus status)
    {
        return status == ClauseStatus.Active;
    }
}
