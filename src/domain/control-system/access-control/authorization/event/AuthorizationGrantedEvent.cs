using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Authorization;

public sealed record AuthorizationGrantedEvent(
    AuthorizationId Id,
    string SubjectId,
    IReadOnlySet<string> RoleIds,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo) : DomainEvent;
