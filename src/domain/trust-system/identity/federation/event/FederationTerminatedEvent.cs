using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Federation;

public sealed record FederationTerminatedEvent(FederationId FederationId) : DomainEvent;
