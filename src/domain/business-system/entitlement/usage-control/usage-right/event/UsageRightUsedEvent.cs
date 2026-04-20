namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public sealed record UsageRightUsedEvent(UsageRightId UsageRightId, Guid RecordId, int UnitsUsed);
