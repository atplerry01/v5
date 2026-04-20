namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public sealed class UsageRecord
{
    public Guid RecordId { get; }
    public int UnitsUsed { get; }

    public UsageRecord(Guid recordId, int unitsUsed)
    {
        if (recordId == Guid.Empty)
            throw new ArgumentException("RecordId must not be empty.", nameof(recordId));

        if (unitsUsed <= 0)
            throw new ArgumentException("UnitsUsed must be greater than zero.", nameof(unitsUsed));

        RecordId = recordId;
        UnitsUsed = unitsUsed;
    }
}
