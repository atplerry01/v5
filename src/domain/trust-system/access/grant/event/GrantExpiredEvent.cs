using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Grant;

public sealed record GrantExpiredEvent(GrantId GrantId) : DomainEvent;
