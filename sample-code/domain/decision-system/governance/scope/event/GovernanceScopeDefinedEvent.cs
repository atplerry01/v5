using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.Scope;

public sealed record GovernanceScopeDefinedEvent(
    Guid ScopeId,
    string ScopeType,
    Guid TargetId,
    string AuthorityLevel) : DomainEvent;
