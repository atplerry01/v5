using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Grant;

public sealed record GrantRevokedEvent(GrantId GrantId) : DomainEvent;
