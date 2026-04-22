using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed record TrustActivatedEvent(TrustId TrustId) : DomainEvent;
