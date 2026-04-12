namespace Whycespace.Domain.BusinessSystem.Agreement.Clause;

public sealed class IsValidClauseSpecification
{
    public bool IsSatisfiedBy(ClauseId id, ClauseType clauseType)
    {
        return id != default && Enum.IsDefined(clauseType);
    }
}
