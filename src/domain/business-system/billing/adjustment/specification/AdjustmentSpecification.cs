namespace Whycespace.Domain.BusinessSystem.Billing.Adjustment;

public sealed class HasReasonSpecification
{
    public bool IsSatisfiedBy(string? reason)
    {
        return !string.IsNullOrWhiteSpace(reason);
    }
}
