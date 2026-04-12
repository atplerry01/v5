namespace Whycespace.Domain.BusinessSystem.Entitlement.Quota;

public sealed class QuotaUsage
{
    public Guid UsageId { get; }
    public int UnitsConsumed { get; }

    public QuotaUsage(Guid usageId, int unitsConsumed)
    {
        if (usageId == Guid.Empty)
            throw new ArgumentException("UsageId must not be empty.", nameof(usageId));

        if (unitsConsumed <= 0)
            throw new ArgumentException("UnitsConsumed must be greater than zero.", nameof(unitsConsumed));

        UsageId = usageId;
        UnitsConsumed = unitsConsumed;
    }
}
