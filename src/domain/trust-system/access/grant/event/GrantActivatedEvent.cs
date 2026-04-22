using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Grant;

public sealed record GrantActivatedEvent(GrantId GrantId) : DomainEvent;
