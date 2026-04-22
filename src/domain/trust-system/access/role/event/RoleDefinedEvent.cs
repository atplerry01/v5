using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Role;

public sealed record RoleDefinedEvent(RoleId RoleId, RoleDescriptor Descriptor, Timestamp DefinedAt) : DomainEvent;
