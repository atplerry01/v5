using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed record FederationGraphVersionedEvent(
    Guid FederationId,
    string GraphHash,
    long FederationVersion,
    string? PreviousGraphHash,
    string? ChangeReason) : DomainEvent;
