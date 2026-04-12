namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageRight;

public sealed record UsageRightUsedEvent(UsageRightId UsageRightId, Guid RecordId, int UnitsUsed);
