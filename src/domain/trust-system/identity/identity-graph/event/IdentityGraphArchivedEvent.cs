using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed record IdentityGraphArchivedEvent(IdentityGraphId IdentityGraphId) : DomainEvent;
