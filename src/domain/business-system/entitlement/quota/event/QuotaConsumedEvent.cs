namespace Whycespace.Domain.BusinessSystem.Entitlement.Quota;

public sealed record QuotaConsumedEvent(QuotaId QuotaId, Guid UsageId, int UnitsConsumed);
