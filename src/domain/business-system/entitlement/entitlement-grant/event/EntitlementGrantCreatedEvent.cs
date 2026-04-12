namespace Whycespace.Domain.BusinessSystem.Entitlement.EntitlementGrant;

public sealed record EntitlementGrantCreatedEvent(EntitlementGrantId EntitlementGrantId, GrantSubjectId SubjectId, EntitlementRightId RightId);
