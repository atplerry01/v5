using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public sealed class UsageRecord
{
    public UsageRecordId RecordId { get; }
    public int UnitsUsed { get; }

    public UsageRecord(UsageRecordId recordId, int unitsUsed)
    {
        Guard.Against(recordId == default, "RecordId must not be empty.");
        Guard.Against(unitsUsed <= 0, "UnitsUsed must be greater than zero.");

        RecordId = recordId;
        UnitsUsed = unitsUsed;
    }
}
