using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.AccessPolicy;

public sealed record AccessPolicyDefinedEvent(
    AccessPolicyId Id,
    string Name,
    string Scope,
    IReadOnlySet<string> AllowedRoleIds) : DomainEvent;
