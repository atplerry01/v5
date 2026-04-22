using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Role;

public sealed record RoleDeprecatedEvent(RoleId RoleId) : DomainEvent;
