namespace Whycespace.Domain.BusinessSystem.Billing.Statement;

public sealed class CanCloseStatementSpecification
{
    public bool IsSatisfiedBy(StatementStatus status)
    {
        return status == StatementStatus.Issued;
    }
}
