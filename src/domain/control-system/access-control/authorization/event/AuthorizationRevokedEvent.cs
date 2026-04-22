using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Authorization;

public sealed record AuthorizationRevokedEvent(AuthorizationId Id) : DomainEvent;
