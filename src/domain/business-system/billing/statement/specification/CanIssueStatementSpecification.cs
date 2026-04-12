namespace Whycespace.Domain.BusinessSystem.Billing.Statement;

public sealed class CanIssueStatementSpecification
{
    public bool IsSatisfiedBy(StatementStatus status)
    {
        return status == StatementStatus.Draft;
    }
}
