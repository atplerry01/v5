using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed record TrustSuspendedEvent(TrustId TrustId) : DomainEvent;
