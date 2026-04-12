namespace Whycespace.Domain.BusinessSystem.Entitlement.Right;

public sealed record RightCreatedEvent(RightId RightId, RightScopeId ScopeId, string Capability, string Constraints);
