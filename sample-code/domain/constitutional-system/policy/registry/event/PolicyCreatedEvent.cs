using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record PolicyCreatedEvent(
    Guid PolicyId,
    string Name,
    Guid ScopeId) : DomainEvent;
