namespace Whycespace.Domain.BusinessSystem.Billing.Statement;

public sealed class IsNonEmptyStatementSpecification
{
    public bool IsSatisfiedBy(int lineCount)
    {
        return lineCount > 0;
    }
}
