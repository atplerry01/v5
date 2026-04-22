using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Federation;

public sealed record FederationEstablishedEvent(FederationId FederationId, FederationDescriptor Descriptor, Timestamp EstablishedAt) : DomainEvent;
