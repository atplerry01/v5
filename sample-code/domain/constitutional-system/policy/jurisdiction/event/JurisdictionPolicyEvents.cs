using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Jurisdiction;

public sealed record JurisdictionPolicyCreatedEvent(Guid PolicyId, string JurisdictionCode, string RegionId) : DomainEvent;
public sealed record OverlayRuleAddedEvent(Guid PolicyId, string PolicyAction, OverlayEffect Effect, int Priority) : DomainEvent;
public sealed record JurisdictionPolicyActivatedEvent(Guid PolicyId) : DomainEvent;
public sealed record JurisdictionPolicySuspendedEvent(Guid PolicyId) : DomainEvent;
public sealed record JurisdictionPolicyRetiredEvent(Guid PolicyId) : DomainEvent;
