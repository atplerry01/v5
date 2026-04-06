using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed record FederationConflictDetectedEvent(
    Guid FederationId,
    Guid SourcePolicyId,
    Guid TargetPolicyId,
    string ConflictReason) : DomainEvent;
