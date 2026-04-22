using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Federation;

public sealed record FederationSuspendedEvent(FederationId FederationId) : DomainEvent;
