namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageRight;

public sealed record UsageRightCreatedEvent(UsageRightId UsageRightId, UsageRightSubjectId SubjectId, UsageRightReferenceId ReferenceId, int TotalUnits);
