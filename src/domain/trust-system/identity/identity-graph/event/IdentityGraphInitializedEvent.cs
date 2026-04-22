using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed record IdentityGraphInitializedEvent(IdentityGraphId IdentityGraphId, IdentityGraphDescriptor Descriptor, Timestamp InitializedAt) : DomainEvent;
