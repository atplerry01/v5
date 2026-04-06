using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed record FederationGraphCreatedEvent(
    Guid FederationId,
    string GraphHash,
    int NodeCount,
    int EdgeCount) : DomainEvent;
