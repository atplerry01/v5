using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed record FederationGraphUpdatedEvent(
    Guid FederationId,
    string GraphHash) : DomainEvent;
