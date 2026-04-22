using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed record ProfileCreatedEvent(
    ProfileId ProfileId,
    ProfileDescriptor Descriptor,
    Timestamp CreatedAt) : DomainEvent;
