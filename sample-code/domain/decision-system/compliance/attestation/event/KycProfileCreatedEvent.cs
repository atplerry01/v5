namespace Whycespace.Domain.DecisionSystem.Compliance.Attestation;

using Whycespace.Domain.SharedKernel;

public sealed record KycProfileCreatedEvent(
    Guid ProfileId,
    Guid IdentityId,
    Guid JurisdictionId) : DomainEvent;
