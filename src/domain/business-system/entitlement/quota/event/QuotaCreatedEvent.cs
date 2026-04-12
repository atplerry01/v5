namespace Whycespace.Domain.BusinessSystem.Entitlement.Quota;

public sealed record QuotaCreatedEvent(QuotaId QuotaId, QuotaSubjectId SubjectId, int TotalCapacity);
